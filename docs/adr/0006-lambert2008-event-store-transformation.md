# 6. Transform the event store to Lambert 2008

Date: 2026-09-08

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008
(EPSG 3812) input while normalizing everything to the event store's reference system.
[ADR 0004](0004-lambert2008-sync-objectcrs.md) put `objectCrs` on the syndication responses.
[ADR 0005](0005-lambert2008-wfs-wms-projections.md) duplicated the WFS and WMS projections so their
tables stay single-SRID whichever system the event store holds. All three deliberately left the
transformation of the event store itself, and the write side, out of scope.

This ADR covers that transformation: the domain change that expresses it, what each projection does
with it, the write side that has to stop undoing it, and the one-shot job that drives it. It mirrors
what address-registry decided in its ADR 0005
([address-registry#1379](https://github.com/Informatievlaanderen/address-registry/pull/1379)) and what
parcel-registry decided in its ADR 0005
([parcel-registry#898](https://github.com/Informatievlaanderen/parcel-registry/pull/898)).

## Decision

### One command per stream, two events

`TransformToLambert2008` takes a `BuildingPersistentLocalId` and nothing else — the transformation has
nothing to decide per building. One command per building stream, which is also one command per
building.

It applies two events, and neither name is an invention: the contracts
`Be.Vlaanderen.Basisregisters.GrAr.Contracts.BuildingRegistry.BuildingGeometryCrsWasChanged` and
`BuildingUnitPositionCrsWasChanged` already exist in the version of GrAr.Contracts this repository
references, so the Kafka messages and their shapes were settled elsewhere and the domain events mirror
them field for field.

| domain event | mirrors | covers |
|---|---|---|
| `BuildingGeometryCrsWasChanged` | `BuildingMeasurementWasChanged` | the building geometry, and the units whose position is derived from it |
| `BuildingUnitPositionCrsWasChanged` | `BuildingUnitPositionWasCorrected` | a unit holding a position of its own |

Mirroring the counterparts is the decision that made everything downstream cheap. Every existing
`BuildingMeasurementWasChanged` and `BuildingUnitPositionWasCorrected` handler touches only fields the
two events share, so each new projection handler is a copy of its counterpart rather than a
hand-written variant. That is roughly thirty files; a geometry-only event would have meant thirty
hand-written variants instead.

`BuildingGeometryCrsWasChanged` restates the geometry method for the same reason, but the aggregate
carries the old one over: the transformation re-expresses the geometry, it does not turn an outlined
building into a measured one.

### The aggregate method is deliberately unguarded

`Building.TransformToLambert2008()` has no removal guard and no status guard, unlike `ChangeOutline`.
It is not an edit of the building but a change of the reference system its geometry is expressed in,
and it has to reach every building the event store holds — removed, not realized and demolished ones
included — or the event store would be left holding both systems indefinitely. The same goes for the
units it walks: a geometry change only reaches planned and realized units, this reaches all of them.

It also does not run `GuardOutline`. That guard requires a polygon of at least 1 m², which the
transformation does not change, and it required SRID 31370, which is precisely what the transformation
leaves behind.

A building whose geometry and unit positions are already Lambert 2008 applies nothing. That is what
makes re-running the transformation over a stream a no-op rather than a double transform, and it is
what the migrator's restart-heavy operating model depends on.

Unused common units are left alone. They only ever leave `BuildingWasMigrated`, are never added back
to `BuildingUnits`, and are not projected, so nothing reads their positions.

### `TransformFromLambert72To08`, not `EnsureLambert08`

`EnsureLambert08` only transforms geometries that actually fall inside Flanders and *relabels*
everything else. For a projection that is harmless. For the event store it would silently corrupt any
geometry outside the envelope — writing Lambert 72 coordinates under SRID 3812, ~500 km from where the
building is. `GeometryReferenceSystem.ToReferenceSystem` therefore transforms unconditionally.

Which system a geometry is currently in is read from its SRID, exactly as `GmlGeometryNormalizer`
already does (ADR 0003). Every geometry reaching this point was read either from GML carrying its own
`srsName` or from persisted EWKB, so the system is not a guess. Parcel-registry decides this from the
coordinates instead, because its GRB reader labels every polygon 31370 by construction;
building-registry has no such path.

### An absent SRID is Lambert 72, not an error

The event store holds geometries written before it wrote EWKB, and those carry no SRID at all. They are
Lambert 72 by definition, and the codebase already reads them that way — `WKBReaderFactory.CreateForEwkb`
falls back to the Lambert 72 reader for them, and `BuildingGeometry.Center` fills in 31370 when the
bytes gave it nothing.

`GeometryReferenceSystem.ReferenceSystem()` is that same rule in one place, and `ToReferenceSystem` goes
through it rather than trusting `Geometry.SRID` directly. The difference is not academic: which SRID an
unlabelled geometry comes back with depends on the reader. `WKBReaderFactory.CreateForEwkb` hands back
31370, because the Lambert 72 reader's geometry factory stamps its own SRID — but a plain `WKBReader` on
the default geometry services, which is what `BuildingGeometry` reads with, returns **-1**. Rejecting
that would have made the helper throw on exactly the legacy rows the transformation exists for, for any
caller that did not happen to read through `CreateForEwkb`.

Transforming an unlabelled geometry produces one labelled 3812, so the transformation also fixes the
missing SRID. Asking for the system it is already in relabels it rather than handing back an unlabelled
geometry, for the same reason.

`IsSupported`, which `Building.GuardPolygon` uses, stays strict: it is a predicate on a *label*, and at
the write boundary an unlabelled geometry should be rejected rather than assumed. Nothing reaches that
guard unlabelled today — every path into it reads through `Building.ReadGeometry` — and this keeps the
guard's behaviour identical to the Lambert-72-only one it replaced.

### The building geometry is not rounded, positions are rounded to centimetres

A building outline or GRB measurement is a boundary whose vertices carry far more decimals than a
centimetre. Rounding them would move the boundary rather than tidy it, so the transformation leaves
them at full precision — as parcel-registry does for its parcel geometries.

A building unit position is a single point, and the BackOffice already persists it at centimetre
precision: `GeometryExtensions.ConvertToGml(false)`, which `GmlGeometryNormalizer` re-serializes
through, writes a point with 2 decimals (ADR 0003). Rounding here keeps a transformed position
identical to the same position normalized on the way in, and drops the transform noise below the
precision anyone reads it at. Address-registry rounds its address positions for the same reason.

### A derived position is recomputed, not transformed

A unit with position method `DerivedFromObject` takes `BuildingGeometry.Center` of the **transformed**
geometry, not the transform of the old centroid.

The two differ by well under a millimetre, and either would be defensible as "the position did not
move". Recomputing is the one that stays self-consistent: `RepairBuilding` and every geometry change
derive the position from the geometry they are given, so had the transformation carried a transformed
old centroid, the first `RepairBuilding` after the conversion would have emitted a position correction
for every derived unit in the register.

### A position the rounding pushes out of its building becomes derived

Rounding a position to centimetres can, for a position within a few millimetres of the boundary, put
it just outside the transformed geometry. `ChangeMeasurement` and `ChangeOutline` already re-derive
such positions — that is what `BuildingUnitPersistentLocalIdsWhichBecameDerived` on the counterpart
events is for — and the transformation does the same, which is why the field is on
`BuildingGeometryCrsWasChanged` at all. It is expected to be very rare.

The condition is deliberately narrow: **inside before and outside after**. A position that was already
outside its building beforehand is not something this transformation caused, and re-deriving it here
would be an edit rather than a reprojection, so it is left classified as it is.

### The projections do not report it as a change

Every projection updates the geometry it holds — the whole point is that readers see the reference
system the event store now holds — but none of them bumps the version or the `LastChangedOn` the
object is served with, and the feed emits no cloud event. A reprojection is not a change to the
building, and reporting it as one would wake every consumer for every building in the register.

Two consequences follow from that:

- The **Oslo snapshot producers** pass `matchOnHashOnly: true` to `FindMatchingSnapshot`. The snapshot
  they wait for never carries the event's timestamp, precisely because the projections do not write
  it, so only the hash can be matched on. This is what the GrAr 26.1.0 bump is for.
- The **detail projections** do update their hash. It tracks the aggregate's `LastEventHash`, and the
  aggregate did append an event.

The **syndication feed** does publish an entry, since both events are tagged `EventTag.For.Sync`, but
it carries over the previous `LastChangedOn` rather than the event's timestamp.

Handlers also have to cope with rows that are not there. The CRS events reach removed buildings and
removed units, unlike every other geometry event, and several projections delete those rows —
`Projections.Wms`, `Projections.Wfs` and `Projections.Extract` outright, and the syndication item drops
removed units from its unit collection. Those handlers guard rather than assume.

### The write side: one toggle, normalized on the way in

The transformation is worthless on its own: without the write side the first edit after the conversion
would write that building straight back to Lambert 72, and the conversion would unwind itself building
by building.

`UseLambert2008EventStoreToggle` says which reference system the event store holds, and
`GmlGeometryNormalizer` converts every incoming geometry to it before the SQS message is created
(ADR 0003). Three things had to change behind that:

- **`Building.GuardPolygon`** required SRID 31370, which is exactly what the transformation leaves
  behind. It now accepts either supported reference system; which one the store actually holds is the
  toggle's business, not the aggregate's.
- **`GmlHelpers.ToExtendedWkbGeometry`** force-set the SRID to Lambert 72 after reading the GML. That
  silently relabelled a Lambert 2008 geometry rather than rejecting it, persisting coordinates ~500 km
  from where the building is. It now keeps the reference system the `srsName` declares, and
  `ReadGeometry` throws on an unsupported or missing one.
- **`BuildingGeometry.Center`** went through `ExtendedWkbGeometry.CreateEWkb`, which validates the
  EWKB's SRID against an expected one that defaulted to Lambert 72 — so it *threw* the moment the
  geometry was Lambert 2008. It now carries the geometry's own SRID over, falling back to Lambert 72
  for SRID-less bytes, which is what it effectively did before.

Every persisted geometry the write side reads back now goes through `WKBReaderFactory.CreateForEwkb`
rather than the Lambert 72 reader.

There is a trap worth recording. `Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology` declares a
`WKBReaderFactory` of its own, and a using directive for that namespace outranks
`BuildingRegistry.WKBReaderFactory` from the enclosing namespace. GrAr's version *throws* on SRID-less
EWKB instead of falling back, and it would compile and pass every test that used a normal EWKB
geometry. `Building.ReadGeometry` therefore qualifies the call.

### The migrator

`BuildingRegistry.Migrator.Lambert2008` is a one-shot console app, scaffolded on address-registry's.
It pages the `Streams` table on its own internal id, loads each `building-` stream, and dispatches
`TransformToLambert2008` for the ones still holding Lambert 72.

- **Resumable.** `[BuildingRegistryMigrationLambert2008].[ProcessedStreams]` records every stream it
  processed and marks a page complete once every stream in it is recorded. A restart resumes from the
  last completed page and skips the recorded tail of the interrupted one. Streams within a page run in
  parallel, so a recorded high id says nothing about the ids below it — only a completed page does.
- **Dry run by default.** `DryRun` loads and measures every stream but dispatches nothing, and records
  its rows separately from a real run's. Letting a dry run advance the watermark a real run resumes
  from would make that run skip every stream the dry run measured.
- **Measured.** Load and dispatch are timed separately, per stream, and reported as p50/p90/p99/max
  rather than an average — they scale with different things, and a staging run is only extrapolatable
  to production if you can tell which of the two dominates.
- **Bounded.** `MaxPagesPerRun` lets a run do a fixed amount of work and exit on its own, so evaluating
  in between does not mean killing the process mid-page.

## Testing

`test/BuildingRegistry.Tests/AggregateTests/WhenTransformingToLambert2008` covers outlined and measured
buildings, derived and appointed units, removed buildings and removed units, idempotency, the
rounding asymmetry, and both sides of the "pushed outside" rule.

`GeometryHelper.PointInPolygonPushedOutsideByLambert2008Rounding` is a concrete point about a
millimetre inside `ValidPolygon` that the centimetre rounding puts outside the transformed polygon, so
that test is deterministic rather than dependent on where the fixture geometry happens to fall.

`ProjectionsHandlesEventsTests` fails until every projection handles both events, which is what keeps
the copy-the-counterpart rule honest.

## Consequences

- The event store ends up holding Lambert 2008 only, and `FeatureToggles:UseLambert2008EventStore` can
  be flipped.
- Every stream gains one or two events per building, plus one per unit holding its own position. The
  version feed and the syndication feed grow accordingly; the change feed does not.
- Consumers of the Kafka topic see `BuildingGeometryCrsWasChanged` and
  `BuildingUnitPositionCrsWasChanged` for every building.
- The overlap checks in `BuildingGeometryContext` read `[BuildingRegistryLegacy].[BuildingDetailsV2]`,
  which `BuildingDetailV2Projections` now moves along with the event store. They compare a geometry
  from the aggregate against those rows, so the conversion and that projection have to stay in step:
  running the migrator ahead of the projector would make every overlap check compare Lambert 2008
  against Lambert 72 and find nothing.

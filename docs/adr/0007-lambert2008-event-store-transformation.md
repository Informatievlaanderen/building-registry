# 7. Transform the event store to Lambert 2008

Date: 2026-09-08

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008
(EPSG 3812) input while normalizing everything to the event store's reference system.
[ADR 0004](0004-lambert2008-sync-objectcrs.md) put `objectCrs` on the syndication responses.
[ADR 0005](0005-lambert2008-wfs-wms-projections.md) duplicated the WFS and WMS projections so their
tables stay single-SRID whichever system the event store holds.
[ADR 0006](0006-lambert2008-consumers.md) gave the consumed parcel geometry, and
`BuildingDetailsV2.SysGeometry`, a second column in the other reference system, and put the choice of
which one matching compares behind `Lambert2008ConversionCompletedToggle`. All four deliberately left
the transformation of the event store itself, and the write side, out of scope.

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

### This is what fills the second column ADR 0006 added

`BuildingDetailsV2` holds the outline twice: `SysGeometry` pinned to Lambert 72, and a nullable
`SysGeometryLambert2008` beside it (ADR 0006). The second column is empty at deploy and fills only as
buildings change — which, before this transformation, meant "eventually, for the ones that happen to be
edited".

`BuildingGeometryCrsWasChanged` is what fills it for **every** building, so it is what makes
`Lambert2008ConversionCompleted` flippable at all: `Lambert2008MatchingReadiness.Buildings` refuses the
flip while any non-removed building with a `SysGeometry` still has a null `SysGeometryLambert2008`.

The handler calls `SetSysGeometryFromCrsConversion`, not `SetSysGeometry`, which ADR 0006 introduced for
exactly this event. It writes only the Lambert 2008 column: the building does not move here, it is
re-expressed, so the stored Lambert 72 outline is already what it should be and transforming the payload
back would replace it with a round trip of itself.

#### What the migrator being ahead of the projector does and does not mean

The migrator appends to the event store and the projector catches up, so the migrator is always ahead.
That lag is not something to arrange around, and it is harmless here for a specific reason:
`SetSysGeometryFromCrsConversion` never writes `SysGeometry`. The conversion does not invalidate the
Lambert 72 column, so a building whose stream is converted and whose row is not yet projected still has,
in that column, exactly what it will have afterwards. `Lambert2008ConversionCompleted` defaults to false
in all three hosts that read these columns, so that is the column every comparison is using.

The only stale thing during the window is `SysGeometryLambert2008`, and nothing reads it while the
toggle is off. `BuildingDetailsV2.Geometry` is stale too — still the Lambert 72 bytes — but its one
reader, `Api.Oslo` through `ParcelMatching.GetUnderlyingParcels(byte[])`, reads it through
`CreateForEwkb` and normalizes to `MatchingSrid`, so it is right either way.

The ordering that actually matters is therefore not about the migrator at all:

1. `UseLambert2008EventStore` on **before** the migrator runs, or an edit to a converted building writes
   it straight back to Lambert 72 and the conversion unwinds building by building.
2. The projector caught up **before** `Lambert2008ConversionCompleted` goes on. Flipping it while the
   column still has NULLs makes `boundingBox.Intersects(building.SysGeometryLambert2008)` NULL for those
   rows, so they drop out of matching with nothing logged. `Lambert2008MatchingReadiness` turns that into
   a loud failure on the first Lambert 2008 match in each process.

Two limits of that guard are worth knowing. It checks `BuildingDetailsV2` and nothing else, so green
means "this column has no NULLs", not "the conversion is done" — the event store and the other
projections are not covered. And it excludes removed buildings, because one removed before the column
existed receives no further geometry events and would otherwise pin the guard red forever. That
exclusion leaves a gap in `BuildingMatching.GetUnderlyingBuildings`, which does not filter removed rows:
a removed building with a NULL Lambert 2008 column would be skipped silently. This transformation closes
it — the aggregate method is unguarded and the projection handler does not skip removed rows, so removed
buildings get the column filled like any other.

### The projections do not report it as a change

Every projection updates the geometry it holds — the whole point is that readers see the reference
system the event store now holds — but none of them bumps the version or the `LastChangedOn` the
object is served with, and the feed emits no cloud event. A reprojection is not a change to the
building, and reporting it as one would wake every consumer for every building in the register.

Two consequences follow from that:

- The **Oslo snapshot producers** pass `matchOnHashOnly: true` to `FindMatchingSnapshot`. The snapshot
  they wait for does not carry the event's timestamp, precisely because the projections do not write
  it, so only the hash can be matched on. This is what the GrAr 26.1.0 bump is for.
- The **detail projections** do update their hash. It tracks the aggregate's `LastEventHash`, and the
  aggregate did append an event.

The **syndication feed** does publish an entry, since both events are tagged `EventTag.For.Sync`, but
it carries over the previous `LastChangedOn` rather than the event's timestamp.

#### The one exception: a unit that became derived

A unit in `BuildingUnitPersistentLocalIdsWhichBecameDerived` is not being re-expressed. Its geometry
method changed from `AppointedByAdministrator` to `DerivedFromObject` and its position moved to the
building's centre — a change in either reference system, and one a consumer has to be told about. Those
units therefore get a version like any other change, and the building unit feed produces a cloud event
for them carrying the position and geometry method attributes.

Every handler that walks `BuildingUnitPersistentLocalIds.Concat(...WhichBecameDerived)` splits on that
set for this reason: same position write for both, version and method only for the ones that became
derived. It stays rare — the aggregate only reclassifies a unit the centimetre rounding pushed outside
its building — so this does not undo the point of the previous section.

#### Nothing is published about a removed building or unit

The CRS events reach removed buildings and removed units, unlike every other geometry event, and that
cuts two ways.

Handlers have to cope with rows that are not there: `Projections.Wms`, `Projections.Wfs` and
`Projections.Extract` delete those rows outright, and the syndication item drops removed units from its
unit collection. Those handlers guard rather than assume.

And where a row does survive, it must not be published. A removed building or unit is not in the feed
and not in the syndication feed, and the conversion is not a reason to put it back:

- **Syndication** creates no entry at all for a removed building. The item carries no removed flag, so
  the guard reads the latest entry's `ChangeType` — reliable here because a building removal has no
  correction event, so nothing un-removes a building, and every other event that could follow one is
  guarded against removed buildings in the aggregate. Removed *units* need no guard: the item has
  already dropped them.
- **The feed** keeps the document current — it is the register's own copy, and leaving it in Lambert 72
  would be a lie — but emits no cloud event for a removed unit.
- **The aggregate does not re-derive a removed unit.** Becoming derived is a real change and gets
  published as one, so it is not something to do to an object nothing is published about. A removed
  unit pushed outside its building by the rounding keeps its own position, re-expressed. That makes the
  feed's guard defensive rather than load-bearing, which is where the rule belongs: in the domain, not
  repeated across a dozen projections.

### The extract stays in Lambert 72

`Projections.Extract` feeds the published shapefiles, which are Lambert 72 and stay that way. It is
therefore the one place where the right response to a re-expression of a geometry it already holds is
to do nothing: the extract's copy is correct before and after.

`BuildingGeometryCrsWasChanged` and `BuildingUnitPositionCrsWasChanged` are handled with `DoNothing`
for that reason — with one exception, the same one as above. A unit that became derived did move, in
Lambert 72 as much as in Lambert 2008, so the building unit extract writes it, bringing the payload
back to Lambert 72 first, and gives it a version.

### GRB is told about a building in the system GRB works in

`AnoApiProxy` sends a realized outlined building to GRB's ANO API as GeoJSON, which carries no SRID at
all — so what GRB receives is decided entirely by the coordinates written into it, with nothing
downstream to catch a mismatch on this side.

This register is expected to convert to Lambert 2008 before GRB does, so the geometry is brought to
`UseLambert2008GrbToggle.GrbSrid` on the way out rather than sent as persisted. Off, and therefore
Lambert 72, is both the default and the current state.

That is a third toggle rather than a reading of one of the other two, because all three move
independently: `UseLambert2008EventStore` says what this register persists,
`Lambert2008ConversionCompleted` says what spatial matching compares in once the registers this one
compares against have converted, and this one says what GRB expects.

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
- `BuildingDetailsV2.SysGeometryLambert2008` goes from "fills as buildings happen to change" to fully
  populated, which is what `Lambert2008MatchingReadiness.Buildings` waits for and therefore what
  unblocks `FeatureToggles:Lambert2008ConversionCompleted` (ADR 0006).
- The rollout has two ordering constraints, and neither is between the migrator and the projector:
  `UseLambert2008EventStore` goes on before the migrator runs, and `Lambert2008ConversionCompleted` only
  after the projector has caught up. The projector lagging the migrator is normal and unobservable,
  because the conversion never touches the Lambert 72 column the toggle-off path compares against.

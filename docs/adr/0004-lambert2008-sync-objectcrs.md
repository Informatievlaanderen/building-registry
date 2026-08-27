# 4. Let the caller pick the syndication object's reference system, through `objectCrs`

Date: 2026-08-25

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008 (EPSG 3812)
input while normalizing everything to the event store's reference system, which is Lambert 72 (EPSG 31370)
for as long as `FeatureToggles:UseLambert2008EventStore` is off.

The event store conversion itself is still to come. When it lands, `ExtendedWkbGeometry` on the events carries
SRID 3812 instead of 31370, and everything reading those events has to cope. The conversion emits a
geometry-change event per building, so a mix of the two reference systems is real but bounded, and consumers
have to survive it.

Geometries are persisted as EWKB, which carries its own SRID. So a reader never has to *assume* a reference
system — it only has to stop hardcoding one.

This ADR covers **one** reader: the syndication feed under `Api.Oslo/Building/V2/Sync`, served by
`GET gebouwen/sync`. It mirrors the decision parcel-registry took in its ADR 0003
([parcel-registry#890](https://github.com/Informatievlaanderen/parcel-registry/pull/890)) and
address-registry in its ADR 0004
([address-registry#1375](https://github.com/Informatievlaanderen/address-registry/pull/1375)).

Handled separately, and **explicitly not** in scope here: the other Oslo responses (V2 detail/list, V3), the
projections (`Legacy`, `Integration`, `Wfs`, `Wms`, `Extract`, `Feed`), the producers, the consumers, and the
write side.

### Why the feed is the one that cannot follow the event store

The syndication object carries its geometry as `GrAr.Legacy.SpatialTools.GmlPolygon` (the building outline)
and `GmlPoint` (each embedded building unit's position). Neither type has an `srsName` member — the string
`srsName` does not occur anywhere in `Be.Vlaanderen.Basisregisters.GrAr.Legacy` — so the object is a bare
`posList` / `pos` and cannot say which reference system it is in.

Letting the object silently follow the event store would therefore move every consumer's coordinates ~500 km
with nothing in the payload to signal it. This one cannot be solved downstream, so the choice is handed to
the caller.

`BuildingSyndicationProjections` writes `message.Message.ExtendedWkbGeometry.ToByteArray()` straight into
`Geometry` / `PointPosition` and never parses the bytes, so the projection is already reference-system
agnostic and needs nothing. Only the read side changes.

## Decision

### The `objectCrs` filter

A new filter on `BuildingSyndicationFilter`:

- `3812` → the object's geometry is emitted in Lambert 2008: transformed if the store still holds Lambert 72,
  passed through once it holds Lambert 2008.
- **anything else — an unrecognised value, an empty one, or no filter at all → Lambert 72**: passed through
  while the store holds Lambert 72, transformed back once it holds Lambert 2008.

The default is what makes the conversion invisible: every existing consumer keeps receiving Lambert 72 before
and after, without changing a thing. Only a caller that opts in sees Lambert 2008.

An unrecognised value falls back rather than returning 400, so the feed never breaks on a typo. The cost is
that `objectCrs=EPSG:3812` silently yields Lambert 72; only the exact string `3812` (trimmed) selects
Lambert 2008. `ObjectCrs.ToSrid` is the single place that mapping lives, and
`GivenObjectCrsFilter.ThenOnlyTheExactValue3812SelectsLambert2008` pins the accepted spellings, so widening
them later is a one-line change with a test that documents it.

`BuildingSyndicationFilter` is populated from the `X-Filtering` header, so exposing `objectCrs` as a query
parameter needs the same gateway mapping that `embed` and `from` already rely on — that part lives outside
this repository.

### Both geometries in the object follow it

There is no building unit syndication feed; a unit only ever appears embedded in a building's object. Its
`GmlPoint` is as silent about its reference system as the building's `GmlPolygon`, and emitting an outline in
Lambert 2008 beside positions in Lambert 72 would be incoherent. So `BuildingUnitHelpers.GetBuildingUnitPoint`
takes the same `objectSrid` as `BuildingHelpers.GetBuildingPolygon`, and one object is wholly in one reference
system.

### `SyncGeometry`

Both helpers read through it: `BuildingHelpers` through `OutlineToRequestedCrs` and `BuildingUnitHelpers`
through `PositionToRequestedCrs`, over one private core that reads the EWKB in the reference system the bytes
carry and puts it in the requested one. Three properties worth stating:

- **Only the object is reprojected.** The embedded `event` is the event store's own payload, emitted verbatim
  at every position, whatever `objectCrs` says. A feed replayed for auditing therefore still shows what was
  actually stored, including the conversion event itself.
- **Only a geometry that has to move is touched.** One already in the requested system is passed through, so
  today's output is byte-for-byte unchanged. `WhenNotRequested_ThenLambert72SourceIsUnchanged` asserts the
  posList at its full 11 decimals for exactly that reason.
- **An outline is not rounded, a position is.** The feed emits both: the building's polygon and, inside it,
  each unit's point. Rounding a point moves it by at most half a centimetre and nothing downstream measures
  it — that is the case the address registry rounds for, and `PositionToRequestedCrs` keeps it. Rounding a
  polygon moves every vertex and so its area, so `OutlineToRequestedCrs` takes the transform at the precision
  it produces. The two share one private core, so the pass-through rule above is stated once.

### `BuildingRegistry.WKBReaderFactory.CreateForEwkb`

`GrAr.Common`'s `WKBReaderFactory.CreateForEwkb` throws `ArgumentException("No SrID found in EWKB")` when the
bytes carry no SRID. Everything written through `ExtendedWkbGeometry.CreateEWkb` does carry one, but its
`byte[]` and hex constructors do not enforce that, and geometries predating the event store writing EWKB do
not carry an SRID.

`BuildingRegistry.WKBReaderFactory.CreateForEwkb` wraps it and falls back to the Lambert 72 reader in that
case. **This is the single place where "no SRID means Lambert 72" is decided.** It replaces what
`MunicipalityGeometryRepository` already open-codes inline; that call site is left alone here because it also
needs the SRID itself, and is out of this ADR's scope.

It shares its name with `Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory`, which needs
care. C# resolves a simple name by walking outward from the innermost namespace, and **at each level it
consults that declaration's `using` directives before moving out**. So in a file that imports the GrAr
namespace, `WKBReaderFactory` binds to *GrAr's* — not to `BuildingRegistry.WKBReaderFactory` from the
enclosing namespace — with no ambiguity error and no warning. That is a silent trap, because the two differ in
exactly the case that matters.

`SyncGeometry` needs `GrAr.Common.NetTopology` for `SystemReferenceId`, so it carries an explicit
`using WKBReaderFactory = BuildingRegistry.WKBReaderFactory;` alias. A using-alias is consulted before
imported namespaces, so this pins the binding in the file rather than leaving it to be inferred — the same
thing `Building/V3/Detail/BuildingDetailHandler.cs` already does.
`GivenObjectCrsFilter.WhenPersistedWithoutSrid_ThenItIsReadAsLambert72` is what catches a lost alias: it fails
with `ArgumentException: No SrID found in EWKB` rather than compiling to something subtly different.

The pre-existing `ExtendedWkbGeometry.CreateEWkb` call to `CreateForEwkb` binds to GrAr's, through the same
rule, and is deliberately left that way: it checks `TryReadSrid` itself before calling, so the throwing
overload is the correct one there.

### No new package reference

`Be.Vlaanderen.Basisregisters.GrAr.CrsTransform` is already reachable from `Api.Oslo` through its
`Projections.Feed` project reference, and is already used there by the V3 detail handlers. No direct
`PackageReference` is added, so `packages.lock.json` does not move.

## Consequences

- While the event store holds Lambert 72, output is byte-for-byte what it was. All new behaviour is on the
  3812 path, which no production data reaches yet. The change is therefore independent of conversion timing.
- SRID-less legacy geometries continue to read as Lambert 72, in one place, deliberately, instead of by
  accident of which factory a call site happened to pick.
- The feed gains an `objectCrs` filter. Callers that do not use it are unaffected in either direction.
- A caller that asks for `3812` gets *both* the building outline and its units' positions in Lambert 2008. A
  consumer that reads only one of the two still sees a consistent object.
- **A geometry that is not `IsValid` is not transformed.**
  `LambertTransformation.EnsureCoordinatesAreInCoordinateSystem` returns such a geometry untouched, so an
  invalid Lambert 2008 outline would be emitted with Lambert 2008 coordinates to a caller that asked for
  Lambert 72 — and, there being no `srsName`, silently. This is left as-is rather than papered over with a
  fixer, which would change the emitted shape; it needs a decision of its own before the store is converted.
  It does not affect anything today, while the store still holds Lambert 72 and no transform runs on the
  default path.
- Buildings whose stored geometry is a multi polygon (incorrectly imported ones) are still dropped by
  `GetBuildingPolygon`'s `as Polygon` cast, exactly as before — the transform now runs before that cast, which
  is wasted work on a geometry that is about to be thrown away, and not worth restructuring for.
- Everything else listed as out of scope above still assumes Lambert 72 and must be handled before the event
  store is converted.

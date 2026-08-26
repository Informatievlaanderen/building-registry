# 5. Duplicate the WFS and WMS projections rather than let their tables go mixed

Date: 2026-08-25

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008 (EPSG 3812)
input while normalizing everything to the event store's reference system, which is Lambert 72 (EPSG 31370)
for as long as `FeatureToggles:UseLambert2008EventStore` is off. [ADR 0004](0004-lambert2008-sync-objectcrs.md)
handled the syndication feed, the one Oslo response whose reference system consumers cannot infer.

The event store conversion itself is still to come. When it lands, `ExtendedWkbGeometry` on the events carries
SRID 3812 instead of 31370, and everything reading those events has to cope. The conversion emits a
geometry-change event per building, so a mix of the two reference systems is real but bounded.

Geometries are persisted as EWKB, which carries its own SRID. So a reader never has to *assume* a reference
system — it only has to stop hardcoding one.

This ADR covers `Projections.Wfs` and `Projections.Wms`. It mirrors what address-registry decided in its
ADR 0004 ([address-registry#1375](https://github.com/Informatievlaanderen/address-registry/pull/1375)), one
version number apart, because building-registry's current tables are `BuildingsV3` and `BuildingUnitsV2`.

Explicitly **not** in scope, and still to do: the other Oslo responses, `Projections.Legacy`,
`Projections.Integration`, `Projections.Extract`, the producers, the consumers, and the write side.

### Why these two cannot simply follow the event store

Both projections feed a geoserver that an external team runs over SQL Server views, and both are anchored to
Lambert 72 in ways that a mixed table breaks silently rather than loudly:

- **WFS** writes `sys.geometry` columns — `[wfs].[BuildingsV3].[Geometry]` and
  `[wfs].[BuildingUnitsV2].[Position]` — with spatial indexes whose `BOUNDING_BOX` is
  `(22279.17, 153050.23, 258873.3, 244022.31)`: Lambert 72 coordinates that every Lambert 2008 row falls
  entirely outside of. SQL Server's spatial methods return `NULL` rather than erroring on an SRID mismatch,
  so a mixed table degrades quietly.
- **WMS** does not store the SRID at all. `SetGeometry` and `SetPosition` write **plain WKB**
  (`geometry.AsBinary()`, which drops the SRID), and the table's `CalculatedGeometry` computed column stamps
  one on: `geometry::STGeomFromWKB([Geometry], 31370)`. The reference system is therefore a property of the
  *schema*, identical for every row, and a Lambert 2008 geometry landing in that column would be labelled
  31370 and served ~500 km away, with nothing in the data to signal it.

So instead of tables that change meaning, there are twice as many tables that do not.

## Decision

### The existing versions are pinned to Lambert 72

`BuildingV3Projections` and `BuildingUnitV2Projections`, in both WFS and WMS, read the geometry SRID-aware
through `BuildingRegistry.WKBReaderFactory.CreateForEwkb` and then call `EnsureLambert72()`. A geometry that
is already Lambert 72 is returned untouched; a Lambert 2008 one is transformed.

Rounding to 2 decimals happens **only on the transformed path**. Geometries are persisted at centimetre
precision and the transform is accurate to that, so rounding drops floating point noise and makes an 08 to 72
geometry read identically to how the same geometry reads while the event store still holds Lambert 72. A
geometry that was already Lambert 72 is not rounded, so today's stored bytes are unchanged, byte for byte.

Once the event store holds Lambert 2008 these projections start transforming, and their tables, spatial
indexes and views carry on unchanged. Consumers of the existing views see nothing at all.

### The new versions are pinned to Lambert 2008

| | WFS | WMS |
|---|---|---|
| Building | `[wfs].[BuildingsV4]`, from `BuildingV4Projections` | `[wms].[BuildingsV4]`, from `BuildingV4Projections` |
| Building unit | `[wfs].[BuildingUnitsV3]`, from `BuildingUnitV3Projections` | `[wms].[BuildingUnitsV3]`, from `BuildingUnitV3Projections` |

Their `ParseGeometry` and `ParsePosition` are `EnsureLambert08(2)`, which transforms and rounds today and
becomes a pass-through once the event store is converted.

The four new projections, their entity configurations and `BuildingUnitV3ProjectionsExtensions` were produced
**mechanically** from their predecessors — a rename of `BuildingV3` to `BuildingV4` and of `BuildingUnitV2`
to `BuildingUnitV3` — so the ~1400 lines of event handling are identical by construction rather than by
review. `ParseGeometry` / `ParsePosition`, the projection name and the table name are the only hand edits.
That keeps the eventual deletion of the old versions a clean delete, at the cost of a duplicate that has to be
kept in step while both exist.

The two run side by side until the geoserver consumers have moved to the new views, after which the old
tables, their projections and their views are deleted in one go.

### `[wfs].[BuildingUnitAddresses]` is not duplicated

It holds two persistent local ids and a count, and `BuildingUnitAddressProjections` never touches a geometry.
It is reference-system agnostic, so both WFS versions read the same table. That is a property worth keeping
rather than a coincidence: a geometry column added there later would need this decision revisited.

### The injected `WKBReader` is gone

All four projections dropped the `WKBReader` from their constructor. It was injected as
`WKBReaderFactory.Create()`, a Lambert 72 reader, from the projector's `ApiModule` — which is exactly the
assumption being removed. The reader now comes from the EWKB, per geometry.

`Projections.Extract` still resolves `WKBReaderFactory.Create()` in that module. It is out of scope here and
keeps its Lambert 72 reader.

### What the migrations add

`AddWfsV4_Lambert2008` and `AddWmsV4_Lambert2008`, one per context, each in a single migration so a rollback
takes the whole version away in one step:

- **WFS**: the two tables as EF generates them, plus a spatial index on `[Geometry]` and one on `[Position]`.
- **WMS**: the two tables, plus
  `ADD CalculatedGeometry AS (geometry::STGeomFromWKB([Geometry], 3812)) PERSISTED` — **3812, not the 31370
  the existing tables use**, since the projection writes plain WKB and this column is what decides the
  reference system the geoserver serves — plus a spatial index on each `[CalculatedGeometry]`.
- **Both**: a `BOUNDING_BOX` of `(522200, 653000, 758900, 744100)`. The Lambert 72 box was converted by
  transforming all four corners — a conformal projection does not map a rectangle to a rectangle — and
  padding the resulting envelope out to the next 100 m. It is the same box address-registry computed from the
  same source box.
- **Views**, one counterpart per existing view, identical to it apart from the source table, and named with
  the new version as its suffix — the same convention the V1-to-V2 overlap used:
  - `[wfs].[GebouwViewV4]` and `[wfs].[GebouweenheidViewV3]`;
  - `[wms].[GebouwViewV4]` plus `GebouwGeplandV4`, `GebouwInAanbouwV4`, `GebouwGerealiseerdV4`,
    `GebouwNietGerealiseerdV4` and `GebouwGehistoreerdV4`;
  - `[wms].[GebouweenheidViewV3]` plus `GebouweenheidGeplandV3`, `GebouweenheidGerealiseerdV3`,
    `GebouweenheidNietGerealiseerdV3` and `GebouweenheidGehistoreerdV3`.

  The WMS status views are `SCHEMABINDING` over their source view, so `Up` creates the source view first and
  `Down` drops it last; both `Down`s drop every view before the tables those views bind to.

Generating them needed `EF.MigrationsHelper` repaired first: it was still on `net9.0` while everything it
references had moved to `net10.0`, and it still referenced `BuildingRegistry.Migrator.Building`, which no
longer exists. It is not in the solution, so nothing had caught either.

## Testing

`BuildingV4Tests` and `BuildingUnitV3Tests`, in both WFS and WMS, are the existing suites copied the same way
the projections were, so the new versions are covered to the same depth instead of being a large untested
copy. One change was needed: their geometry expectations read the event with a Lambert 72 reader, so in the
new versions they go through an `ExpectedGeometry` / `ExpectedPosition` helper that also applies
`EnsureLambert08(2)`.

That does mirror what the projection does, which is why the reference system is asserted against **fixed**
coordinates in `GivenBuildingGeometryInEitherReferenceSystem` and
`GivenBuildingUnitPositionInEitherReferenceSystem` — one class per version, in both registries — rather than
relied on there. Each replays the same physical geometry as both a Lambert 72 and a Lambert 2008 event and
asserts the stored coordinates are the pinned ones either way. For WMS, where the stored bytes carry no SRID,
those tests read them back the way the `CalculatedGeometry` computed column will, so the assertion is about
what the geoserver ends up serving.

`GeometryHelper.CreateEwkbFromWkt(wkt, srid)` is what lets a test hand a projection an event geometry in
either reference system.

## Consequences

- While the event store holds Lambert 72, the existing tables are byte-for-byte what they were, and all the
  new behaviour is on the 3812 path, which no production data reaches yet.
- WFS gains two tables, two spatial indexes and two views; WMS gains two tables, two computed columns, two
  spatial indexes and eleven views. The geoserver team moves over at its own pace and the old versions are
  deleted once nobody reads them.
- The projector runs four more projections, each replaying the full stream once. Expect the initial catch-up
  to cost roughly what the existing WFS and WMS projections cost today.
- Neither table is ever mixed, in either direction, so no spatial index is invalidated and no view has to
  branch on SRID. That is the whole point of paying for the duplicate.
- The duplicates have to be kept in step: a change to `BuildingV3Projections` that is not also made to
  `BuildingV4Projections` silently desynchronizes the two feeds. They are identical apart from
  `ParseGeometry` / `ParsePosition`, so a diff of the two files should show exactly that and nothing else.

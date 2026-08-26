# 6. Hold consumed parcel geometry in both reference systems, and choose the matching one by toggle

Date: 2026-08-26

## Status

Proposed

## Context

[ADR 0005](0005-lambert2008-wfs-wms-projections.md) listed "the consumers" among the things explicitly not
in scope and still to do. This closes that, for both of them.

They turn out to be nothing alike.

### `Consumer.Address` — nothing to do

It never stored a position. This is not a case of reading geometry in a hardcoded reference system; there is
no geometry.

- `AddressConsumerItem` has four fields: `AddressPersistentLocalId`, `AddressId`, `Status`, `IsRemoved`.
- Its migrations are `Initial`, `AddIndexDateProcessed` and `AddOffsetOverride`. The `ConsumerAddress`
  schema has never held a geometry column or a spatial index, and the context never calls
  `UseNetTopologySuite` — the `Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite` reference in the
  csproj is vestigial.
- `AddressKafkaProjection` handles 28 events, none of them positional. `AddressPositionWasChanged` and
  `AddressPositionWasCorrectedV2` are on the topic today and already fall through unhandled: the projection
  has never cared where an address is.
- `CommandHandlingKafkaProjection` translates status, removal and readdressing into building commands.
- `IAddresses` exposes `GetOptional` and `GetAddresses`, both returning
  `AddressData(persistentLocalId, status, isRemoved)`. There is no spatial member, and no counterpart to
  parcel-registry's `FindAddressesWithinGeometry`.
- Nothing anywhere in `src/` references an address position event.

`AddressPositionCrsWasChanged` needs no handler either. It has been in `GrAr.Contracts` since 24.4.0 and this
repository pins 26.0.0, so it deserializes into its real type and the projector finds no handler for it —
exactly what already happens to `AddressPositionWasChanged` in production.

This is recorded rather than left to be re-derived, because "the address consumer" is the obvious place to
look for this problem and the answer is not obvious from the outside.

### `Consumer.Read.Parcel` — a join across two registers

This one is not a consumer that reads geometry in one reference system. It is a **spatial join between two
registers that convert on independent schedules**: parcel geometry arriving over Kafka from parcel-registry,
compared against building geometry that follows this repository's own conversion. Neither side is under this
code's control, and the comparison happens at four places, in two directions.

**Stored**, in `[BuildingRegistryConsumerReadParcel].[ParcelItemsWithCount]`:

| Column | What it is | Read by |
|---|---|---|
| `Geometry` | `sys.geometry`, `GeometryFixer.Fix`'d on write by `ParcelConsumerItem.SetGeometry`, indexed by `SPATIAL_ParcelItems_Geometry` with `BOUNDING_BOX = (22279.17, 153050.23, 258873.3, 244022.31)` | every comparison below |
| `ExtendedWkbGeometry` | the raw published bytes | **nothing** — write-only |

Written by `ParcelKafkaProjection` at four sites (`ParcelWasMigrated`, `ParcelWasImported`,
`ParcelGeometryWasChanged`, `ParcelWasCorrectedFromRetiredToRealized`) through `WKBReaderFactory.Create()`,
which is pinned to Lambert 72. `WKBReader` takes the SRID from the bytes, so it already reads Lambert 2008
correctly — the same accident ADR 0004 declined to rely on — and the column would therefore go mixed
silently the moment parcel-registry converts.

**Compared**, at four points:

*Direction A — a building geometry queries parcel geometry:*

1. `Consumer.Read.Parcel.ParcelMatching.GetUnderlyingParcels(byte[])` reads the building bytes, filters on
   `boundingBox.Intersects(parcel.Geometry)` in SQL, then runs `OverlayNGRobust.Overlay` and `Intersection`
   in memory. Called by **`Api.Oslo` `BuildingDetailHandlerV2` and `BuildingDetailHandler` V3**, with
   `building.Geometry` bytes out of `Projections.Legacy`.
2. `ConsumerParcelContext.GetUnderlyingParcelsUnderBoundingBox(Geometry)`, implementing `IParcels`, runs the
   same SQL predicate but carries `x.Geometry` into `ParcelData` and so **into the domain**, where
   `BuildingRegistry.ParcelMatching` does `geometry.Intersects(parcel.Geometry)` and an area-ratio overlap.
   Reached from **`RealizeAndMeasureUnplannedBuildingLambdaHandler`**, whose building geometry comes from
   GRB GML.

*Direction B — a parcel geometry queries building geometry:*

3. `ParcelKafkaProjection.GetBuildingPersistentLocalIdsToInvalidate` calls
   `Projections.Legacy.BuildingMatching.GetUnderlyingBuildings(parcelGeometry)`, which filters on
   `boundingBox.Intersects(building.SysGeometry)` against `[BuildingRegistryLegacy].[BuildingDetailsV2]` and
   then intersects in memory.

### Three ways a mismatch fails, all of them silent

1. **SQL Server returns `NULL` rather than erroring on an SRID mismatch**, so the bounding-box filter matches
   nothing.
2. **NTS ignores SRID entirely.** This is the more dangerous one and has no counterpart in parcel-registry's
   consumer. `OverlayNGRobust.Overlay`, `Intersects` and `Intersection` will compare a Lambert 72 polygon
   against a Lambert 2008 one, find them ~500 km apart, and return an empty intersection: no exception,
   overlap `0.0`, parcel discarded. Fixing only the SQL predicate would leave this layer wrong.
3. **The spatial index bounding box is Lambert 72**, so Lambert 2008 rows fall outside the tessellated space.

### Why this matters more than it did in parcel-registry

There, a mismatch meant the GRB importer attached no addresses — bad, but batch work behind a pause. Here:

- **`Api.Oslo` building detail** silently returns a building with no `perceelIds`. A live read API, no error.
- **`RealizeAndMeasureUnplannedBuilding`** derives the building's **addresses** from the overlapping parcels.
  A mismatch means a realized building is persisted with no addresses attached. That is data, not a response.
- **`BuildingsToInvalidate`** silently invalidates nothing, so caches go stale with no signal.

There is also no pause available and no way to rebuild: unlike both address consumers,
`Consumer.Read.Parcel` has **no `OffsetOverride`**, so replaying the parcel topic is not a supported
operation in that project today.

## Decision

### `Consumer.Address` is not changed

### `ParcelItemsWithCount` holds the geometry in both reference systems

`ParcelConsumerItem` gains `GeometryLambert2008` alongside `Geometry`, each with its own spatial index. Both
are written on every geometry-bearing event.

The alternative designs are under "Considered and rejected" below. This one is chosen because it is the only
one that makes the eventual move to Lambert 2008 a **configuration change rather than a data migration**. A
single pinned column can be moved to Lambert 2008 only by recreating its spatial index and rewriting every
row — and this table backs a live API, with no pause window and no replay machinery. Pre-populating the
second column is what buys an instant, reversible switch.

`ExtendedWkbGeometry` keeps its current meaning: the bytes exactly as the parcel event store last published
them, in whatever reference system that is. It is updated on every geometry-bearing event, including the CRS
conversion. Nothing reads it, but that is the one thing it could be relied on for if anything ever did.

### The matching reference system is chosen by a toggle, not by the data

```csharp
public sealed class Lambert2008ConversionCompletedToggle
{
    public bool FeatureEnabled { get; }

    public int MatchingSrid => FeatureEnabled
        ? SystemReferenceId.SridLambert2008
        : SystemReferenceId.SridLambert72;
}
```

Read from `FeatureToggles:Lambert2008ConversionCompleted`, and living in `BuildingRegistry` next to
`WKBReaderFactory` — the only project all four comparison points can see, since one of them is the domain's
own `ParcelMatching` and another is `Projections.Legacy.BuildingMatching`. `FeatureToggleOptions` is confined
to `Api.BackOffice`, so each of the three hosts reads the key and registers the instance, as `ApiModule`
already does for the existing toggle.

**This is not `UseLambert2008EventStoreToggle`, and the two must not be conflated.** That one goes on when
the conversion *begins* — it decides which system incoming GML is normalized to before it is written. This
one goes on when the conversion *ends*, in every register this repository compares against: address, parcel
and building. Both are enabled afterwards, which is exactly why they need names that cannot be mistaken for
one another.

### The toggle is a preference, not a correctness switch

Worth stating plainly, because it is the property that makes a manually thrown flag acceptable at all.

Both columns are always populated, and the incoming building geometry is brought to `MatchingSrid` at the
comparison point. So the arithmetic is correct in either system at any moment:

- thrown **early**, while parcels still arrive as Lambert 72: the derived Lambert 2008 column is compared
  against a building geometry transformed to Lambert 2008. Correct — just two transforms that need not have
  happened.
- thrown **late**, after everything is Lambert 2008: the derived Lambert 72 column against a building
  geometry transformed to Lambert 72. Also correct.

A premature flip is inefficient, not wrong. What "conversion completed" actually marks is the point where
matching in Lambert 2008 stops costing extra work and the Lambert 72 column becomes deletable.

It also follows that the flag does **not** need a synchronized rollout across the consumer, `Api.Oslo` and
the Lambda. Each picks its column and normalizes to match; a service left behind is still right.

### Reading, writing and normalizing

- `ParcelKafkaProjection` drops the cached `WKBReaderFactory.Create()` for `CreateForEwkb` per geometry, so
  the reference system comes from the bytes rather than from a reader chosen at construction.
- `ParcelConsumerItem.SetGeometry` fixes **then** transforms, in that order:
  `GeometryFixer.Fix` first, then `EnsureLambert72()` and `EnsureLambert08(2)` from the fixed geometry.
  `LambertTransformation.EnsureCoordinatesAreInCoordinateSystem` returns a geometry that is not `IsValid`
  untouched, so transforming first would stamp an SRID on unmoved coordinates. The fixer is already there
  and a fixed geometry is valid by construction.
- Each of the four comparison points brings its incoming building geometry to `MatchingSrid` before
  comparing — reading through `WKBReaderFactory.CreateForEwkb` where it starts as bytes. **For comparison
  only:** what a command persists is never the transformed copy.

### The conversion event does not rewrite the Lambert 72 column

`ParcelGeometryCrsWasChanged` is handled, and writes `GeometryLambert2008` and `ExtendedWkbGeometry` but not
`Geometry`. The parcel does not move there, it is re-expressed, so the stored Lambert 72 geometry is already
what it should be and transforming the payload back would replace it with a rounded round trip of itself.

The stakes are lower than in the parcel-registry counterpart, which matches on `Touches` where a centimetre
decides the answer; here the comparison is an area ratio against a `0.8 / count` threshold, which centimetres
do not move. The rule is kept anyway, because it follows from what each column means: `Geometry` and
`GeometryLambert2008` are working copies that should not be degraded, `ExtendedWkbGeometry` is whatever was
last published.

### Two guards, because both failures are silent

- **Matching against a column that still has NULLs fails loudly.** This is the one real hazard of a manually
  thrown flag: `Lambert2008ConversionCompleted` enabled before `GeometryLambert2008` is fully populated would
  silently return empty parcel lists. Checked once and memoized — it only ever goes from false to true.
- **A geometry outside both Flanders envelopes fails loudly on the write path.** `LambertTransformation`
  decides by envelope, so a geometry outside both is not transformed at all; it just has an SRID stamped on
  unmoved coordinates.

### Migration

One migration adds `GeometryLambert2008` as a nullable `sys.geometry` column and
`SPATIAL_ParcelItems_GeometryLambert2008`:

```
BOUNDING_BOX = (522200, 653000, 758900, 744100)
```

The existing Lambert 72 bounding box's four corners transformed and the envelope padded to the next 100 m —
the same numbers ADR 0005 derived for the V3/V4 tables, reused rather than re-derived. Grids and
`CELLS_PER_OBJECT` are copied from the Lambert 72 index unchanged.

Nullable because it is empty at deploy and fills as parcel events arrive. Unlike parcel-registry's address
consumer, **there is no free rebuild to catch here**: `ParcelGeometryCrsWasChanged` will fill it for every
parcel when parcel-registry converts, but until then it fills only for parcels that happen to change. That is
fine — the toggle stays off, `Geometry` serves every comparison, and the guard refuses the flip until the
column is complete.

### Direction B depends on Projections.Legacy being uniform

`BuildingMatching.GetUnderlyingBuildings` brings the incoming parcel geometry to `MatchingSrid` like every
other comparison point, and is implemented that way rather than deferred.

That is correct **so long as `Projections.Legacy.BuildingDetailV2.SysGeometry` is uniformly in
`MatchingSrid`** — Lambert 72 while the toggle is off, Lambert 2008 once it is on. The toggle's meaning
already carries that: it asserts every register has finished converting, and Legacy follows the building
event store.

What it does not survive is `SysGeometry` being allowed to hold *both*, which is a live option for
`Projections.Legacy` because ADR 0005 left its reference system undecided. A mixed `SysGeometry` cannot be
matched against in any single system, and no normalization here would fix it.

**So this is a constraint on the Legacy decision, not a blocker on this one:** whatever ADR 0005's successor
decides for `SysGeometry`, it has to be a single system at a time. If it goes mixed, direction B needs
revisiting and the toggle is not enough.

### End state

Once the toggle has been on long enough to trust, a second migration drops `Geometry` and
`SPATIAL_ParcelItems_Geometry`, the matching collapses to one column, and the toggle and the Lambert 72 write
path go with it. Whether `GeometryLambert2008` is then renamed to `Geometry` costs a further migration and is
left open here so that it is decided rather than defaulted into.

## Considered and rejected

### One column pinned to Lambert 72, forever

Correct indefinitely, and the cheapest thing that works: once everything is Lambert 2008 the two transforms
simply cancel out. No migration, no second index, no toggle.

Rejected because Lambert 72 has to be retired eventually, for consistency with every other table in this
repository, and this option has no route there that does not become the next option.

### One column pinned to Lambert 72, moved later by toggle

What a config flag looks like if the second column is not there. Rejected because the flip is a data
migration wearing a configuration flag's clothes: throwing it makes new writes Lambert 2008 while every
existing row stays Lambert 72, leaving the column mixed and the spatial index bounding box wrong, with
nothing erroring — the Oslo building detail simply starts returning buildings with no parcels.

Doing it properly would need an index migration and a rewrite of every row, against a live API with no pause
window and no `OffsetOverride` to replay from.

### One column, allowed to hold both

Rejected by all three failure modes above, and in particular by the second: even with the SQL predicate
made SRID-aware, the in-memory NTS overlay would still silently return empty intersections.

## Consequences

- While parcel-registry holds Lambert 72 and the toggle is off, every comparison is exactly what it is today,
  and `Geometry` is byte-for-byte what it is today. All new behaviour is on the Lambert 2008 path, which no
  production data reaches yet.
- `ParcelItemsWithCount` carries two spatial indexes until Lambert 72 is dropped. Both are maintained only
  when their column changes, so parcel-address events cost nothing extra; the peak is parcel-registry's
  conversion, which is a geometry change on every row and so rebuilds both.
- Moving to Lambert 2008 becomes a configuration change per host, reversible, with no downtime and no
  rebuild. That is the entire return on the second column and index.
- The four comparison points must all resolve `MatchingSrid` from the same toggle. A call site that forgets
  compares in a system the column is not in, and fails in the two silent ways above. This is the thing most
  worth catching in review.
- A geometry that is not `IsValid` is now fixed before it is transformed rather than only before it is
  compared. That changes nothing today and is what keeps the transform honest once one runs.
- Tests must assert SRIDs and coordinates explicitly, and must cover the in-memory overlay as well as the SQL
  predicate: a test that only exercises the bounding-box filter will pass while `OverlayNGRobust` silently
  returns nothing.
- `Be.Vlaanderen.Basisregisters.GrAr.CrsTransform` is already pinned at 26.0.0 in this repository, so no
  package moves.

### Still to do

- `Projections.Legacy`, still open from ADR 0005. Direction B constrains it: `SysGeometry` must be uniform
  in one reference system at a time, never mixed.
- The producers and the write side, likewise still open from ADR 0005.
- `Consumer.Read.Parcel` has no `OffsetOverride`. It is not needed for this change, but its absence is why
  the rebuild-based alternatives were unavailable, and it is worth adding before anything else needs to
  replay that topic.

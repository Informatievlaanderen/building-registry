# 8. Pin the Lambert 72 responses that say they are Lambert 72

Date: 2026-09-10

## Status

Accepted

## Context

[ADR 0005](0005-lambert2008-wfs-wms-projections.md) duplicated the WFS and WMS projections and listed what
it was not doing: *"the other Oslo responses, `Projections.Legacy`, `Projections.Integration`,
`Projections.Extract`, the producers, the consumers, and the write side."*
[ADR 0006](0006-lambert2008-consumers.md) closed the consumers. [ADR 0007](0007-lambert2008-event-store-transformation.md)
closed the write side and recorded what every projection does with the transformation event.

Neither carried the rest of that list forward. ADR 0006's "Still to do" kept `Projections.Legacy`, the
producers and the write side; **the other Oslo responses and `Projections.Extract` dropped off it**, and
ADR 0007 restates the chain without them. So the ADRs read as though only the write side remained, and the
two items that had gone missing were the two that fail silently. `Projections.Feed` was never on any list
at all, in any of the five.

This ADR closes that list. It covers what each of them does when a geometry arrives in Lambert 2008 in
ordinary operation — not what it does with `BuildingGeometryCrsWasChanged`, which is ADR 0007's subject.

The distinction that decides every case below: **a reader that only has to stop hardcoding a reference
system needs nothing, because EWKB carries its own SRID. A reader that publishes a reference-system claim
of its own has to make the claim true.** Two of them make such a claim, and both were making it falsely.

## Decision

### Api.Oslo version 2 detail: transform before the hardcoded `srsName`

`BuildingDetailHandlerV2.GetGml` and `BuildingUnitDetailHandlerV2.GetGml` write

```
srsName="https://www.opengis.net/def/crs/EPSG/0/31370"
```

as a literal, and then the geometry's own coordinates. Nothing between the event store and that
attribute changed the geometry: `BuildingDetailV2Projections` assigns the event's payload verbatim
(`item.Geometry = geometryAsBinary`), including on `BuildingGeometryCrsWasChanged`. So once the event
store holds Lambert 2008, version 2 would answer with Lambert 2008 coordinates under a label that says
Lambert 72 — a lie of about 500 km, in a field a consumer has every reason to trust.

`GetBuildingPolygon` and `GetBuildingUnitPoint` now read through `WKBReaderFactory.CreateForEwkb` and
bring the result to Lambert 72 with `GeometryReferenceSystem.ToReferenceSystem`. A geometry already in
Lambert 72 is returned untouched, so nothing about today's output moves.

**`ToReferenceSystem` rather than `EnsureLambert72()`**, which is what the syndication response uses.
`EnsureLambert72` relabels a geometry falling outside its envelope instead of transforming it — for a
response whose `srsName` is a hardcoded claim, that is the one outcome worse than the bug being fixed.
The syndication object can afford `EnsureLambert72` because it carries no `srsName` at all and the caller
picked the system through `objectCrs` (ADR 0004); this one cannot.

Version 3 is untouched and stays the version that answers in the system the geometry is persisted in,
Lambert 72 equivalent first (`BuildingDetailHandler`, `BuildingUnitDetailHandler`). That split — version
2 pinned, version 3 following the store — is the same one address-registry took.

**Rounding follows the repository's existing rule**, which `GeometryReferenceSystem.PositionRoundingPrecision`
already states: a unit position is rounded to centimetres, a building outline is not. It happens to be
invisible in the response either way — `PointGeometryCoordinateValue` renders `F2` and
`PolygonGeometryCoordinateValue` renders `F11` — but stating it here keeps the two version 2 handlers
saying the same thing as the sync path and the extract.

`GetBuildingPolygon` transforms **after** the `as Polygon` cast, not before: an incorrectly imported multi
polygon is discarded, and transforming one first is work thrown away. The sync path does it the other way
round and ADR 0004 notes it as not worth restructuring for; here the ordering came for free.

### Projections.Extract: pin it, rather than trust that nothing new arrives

`Api.Extract` writes a `.prj` of `ProjectedCoordinateSystem.Belge_Lambert_1972` for both extracts, and a
shape record carries no SRID of its own. So the projection is the only place that can keep the `.prj`
honest — nothing downstream could even detect a mismatch.

ADR 0007 said the extract "is Lambert 72 and stays that way" and handled the transformation event with
`DoNothing` on that basis, which is right: the extract's copy is correct before and after a
re-expression. But nothing enforced it for any *other* event. Both projections took a `WKBReader`
injected as `WKBReaderFactory.Create()` — a Lambert 72 reader — from the projector's `ApiModule`, and
`WKBReader` takes the SRID from the bytes regardless of which factory made it. So a `BuildingWasMeasured`
or `BuildingUnitWasPlannedV2` arriving after the conversion would have been written into the shapefile
with its Lambert 2008 coordinates, under a Lambert 72 `.prj`.

Both projections now drop the injected reader and read per geometry, through a private helper:

- `BuildingExtractV2EsriProjections.ParseGeometry` → `ToReferenceSystem(SridLambert72)`, unrounded.
- `BuildingUnitExtractV2Projections.ParsePosition` → `ToReferenceSystem(SridLambert72, PositionRoundingPrecision)`.

The unit extract's `BuildingGeometryCrsWasChanged` handler already did exactly this inline, for the units
that became derived (ADR 0007); it now goes through the same helper, so the rule is stated once per
projection rather than once per handler.

With this, no `WKBReaderFactory.Create()` is left in the projector's `ApiModule`.

### What needs nothing, and why

Recorded so it does not have to be re-derived a third time.

- **`Projections.Legacy`.** `BuildingDetailV2Projections` stores the event's payload verbatim in
  `Geometry`, SRID included, and hands the decision to whoever reads the column — which is now the two
  version 2 handlers above, pinned, and version 3, which follows the store. Its `SysGeometry` /
  `SysGeometryLambert2008` pair is ADR 0006's and already branches on `IsLambert72()` / `IsLambert08()`.
- **`Projections.Integration`.** Npgsql's NetTopologySuite plugin writes the geometry's SRID into the
  PostGIS `geometry` column, so a row says which system it is in and `ST_SRID` can be branched on. The
  column is deliberately allowed to hold both, with the same consequences for consumers that
  parcel-registry's ADR 0003 sets out — GIST indexes stay valid, the predicate functions are what raise
  `ERROR: Operation on mixed SRID geometries`, and `CASE` is the way to force evaluation order at the cost
  of the index.
- **`Projections.Feed`.** The change feed carries GML in both reference systems already:
  `CreateGeometryValues` and `CreatePositionValues` branch on the geometry's SRID and emit the Lambert 72
  entry followed by the Lambert 2008 one, whichever of the two was stored. So the conversion changes which
  entry was stored and which was derived, never the shape of the response. `MunicipalityGeometryRepository`
  — deferred explicitly by ADR 0004 and never picked up since — matches a geometry against municipality
  boundaries held in **both** systems, selecting with `m.Srid == srid` rather than reprojecting, and reads
  SRID-less EWKB as Lambert 72 by open-coding the same fallback `BuildingRegistry.WKBReaderFactory` wraps.
  Those boundaries are municipality-registry's `geometry_lambert08` columns
  (`20260317055836_AddLambert08`, `20260402123429_AddGeometries2019`, both `NOT NULL`), and the
  repository's `SELECT` names the column unconditionally — a hard cross-repository dependency, already
  satisfied, that nothing in this repository would otherwise reveal.
- **The producers.** `MessageExtensions` passes `message.ExtendedWkbGeometry` into the Kafka contracts as
  the hex it arrived as. Agnostic by construction; there is nothing to decide. The Oslo snapshot producers
  fetch from the version 3 Oslo endpoint through `IOsloProxy`, so they inherit version 3's behaviour.

### `WKBReaderFactory.Create()` elsewhere is fine, and this is what it would buy to change it

`WKBReaderFactory.Create()` — nominally a Lambert 72 reader — is still the reader in
`Projections.Legacy`, `Projections.Integration` and `Projections.Feed`'s `GmlHelpers.ParseGeometry`,
about forty call sites. The sibling registries' ADRs justify moving such call sites onto `CreateForEwkb`
by calling the fact that they work "an accident of the current precision models, not a contract". That
was worth checking rather than inheriting, and it does not survive the check.

`NtsGeometryFactory.CreateNtsGeometryServicesLambert72()` and `CreateNtsGeometryServicesLambert2008()`
are the same three lines twice: a `DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY)`, a
`new PrecisionModel(PrecisionModels.Floating)`, and a default SRID. **The default SRID is the only
difference between them.** So there is no divergence to be exposed to: a change to the precision model
would have to be made to one of an adjacent pair and not the other, and if it were made to both,
`CreateForEwkb` would inherit it just the same.

Measured on the versions this repo pins, reading the same EWKB both ways:

| Input | `Create()` | `WKBReaderFactory.CreateForEwkb` |
|---|---|---|
| EWKB, SRID 31370 | SRID 31370, coordinates bit-identical | the same |
| EWKB, SRID 3812 | SRID 3812, coordinates bit-identical | the same |
| WKB, no SRID | SRID 31370 | SRID 31370 (via our Lambert 72 fallback) |
| EWKB, SRID 4326 | **SRID 4326, passed through silently** | **throws `InvalidOperationException: Unsupported SRID: 4326.`** |

So the refactor buys exactly one thing, and it is not the one the other ADRs claim: **a fail-fast on a
reference system this registry does not support.** Everything else is expressiveness.

That fail-fast is worth almost nothing in two of the three places. `Projections.Feed`'s
`CreateGeometryValues` and `CreatePositionValues` already throw `ArgumentOutOfRangeException` on their
default branch, and `Projections.Legacy`'s `SetSysGeometry` already refuses a geometry outside Flanders in
both systems. **`Projections.Integration` is the one place where a foreign SRID would flow through
unnoticed**, into a PostGIS `geometry` column that consumers branch on with `ST_SRID` — and it is also the
place where nothing upstream can produce one, since the write side normalizes to the event store's system
(ADR 0003) and `GuardPolygon` refuses anything else.

The conclusion is that this is **a consistency change, not a correctness one**. It is worth doing when
those files are next touched for another reason; it is not worth a forty-call-site commit of its own, and
it is not a precondition for converting the event store. Recorded here so the next person does not
re-derive it, or worse, treat the inherited wording as a reason to hurry.

## Consequences

- While the event store holds Lambert 72, every response and every shapefile is byte-for-byte what it was.
  All the new behaviour is on the 3812 path, which no production data reaches yet.
- **Version 2 consumers never see Lambert 2008**, before or after the conversion. Version 3 consumers get
  a second `geometrie` entry once it happens, which is what version 3 has always promised.
- The published shapefiles stay Lambert 72 through the conversion and afterwards, matching their `.prj`,
  with no change to `Api.Extract` — it never touches a coordinate, which is exactly why pinning the
  projection is what keeps the `.prj` honest.
- `BuildingExtractV2EsriProjections` and `BuildingUnitExtractV2Projections` lost their `WKBReader`
  constructor parameter. The projector's `ApiModule` and `ProjectionsHandlesEventsTests` construct them
  without it.
- `BuildingRegistry.Api.Oslo` gained `[assembly: InternalsVisibleTo("BuildingRegistry.Tests")]`, following
  what `BuildingRegistry` already does, so the two version 2 readers can be tested directly. They are
  `internal static` rather than `private static`; nothing else about them is public.
- New tests: `ProjectionTests/Extract/GivenGeometryInEitherReferenceSystem` (the extract had no projection
  tests at all, so it also gets a `BuildingExtractProjectionTest` harness mirroring the WFS one) and
  `Oslo/DetailTests/GivenAGeometryInEitherReferenceSystem`. Both replay the same physical geometry in each
  reference system and assert one Lambert 72 answer, which is the property that would otherwise be lost
  silently.
- **Nothing on ADR 0005's list is still to do.** The `Create()` call sites above are optional tidying, on
  the evidence in this ADR, rather than the last open item they were previously written up as.

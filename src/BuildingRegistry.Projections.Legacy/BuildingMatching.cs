namespace BuildingRegistry.Projections.Legacy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;

    public class BuildingMatching : IBuildingMatching
    {
        private readonly LegacyContext _legacyContext;
        private readonly Lambert2008ConversionCompletedToggle _conversionCompleted;
        private readonly Lambert2008MatchingReadiness _readiness;

        public BuildingMatching(
            LegacyContext legacyContext,
            Lambert2008ConversionCompletedToggle conversionCompleted,
            Lambert2008MatchingReadiness readiness)
        {
            _legacyContext = legacyContext;
            _conversionCompleted = conversionCompleted;
            _readiness = readiness;
        }

        /// <summary>
        /// Finds the buildings under a parcel.
        ///
        /// The incoming parcel geometry is brought to the reference system matching is done in, and the
        /// column held in that same system is compared against, so both sides agree. Neither layer below
        /// would say otherwise if they did not: SQL Server returns NULL rather than erroring when SRIDs
        /// disagree, and NTS ignores SRID entirely — it would compare a Lambert 72 polygon against a
        /// Lambert 2008 one, find them ~500 km apart, and return an empty intersection with no exception at
        /// all. See ADR 0006.
        /// </summary>
        public IEnumerable<int> GetUnderlyingBuildings(Geometry parcelGeometry)
        {
            var matchingSrid = _conversionCompleted.MatchingSrid;
            var useLambert2008 = matchingSrid == SystemReferenceId.SridLambert2008;

            if (useLambert2008)
            {
                _readiness.EnsureVerified(
                    Lambert2008MatchingReadiness.Buildings,
                    _legacyContext.HasIncompleteLambert2008Geometry);
            }

            var matchingGeometry = ToMatchingCrs(parcelGeometry, matchingSrid);
            var boundingBox = matchingGeometry.Factory.ToGeometry(matchingGeometry.EnvelopeInternal);

            // Two near-identical queries rather than one with a conditional predicate: EF has to translate
            // the column into SQL, so which one is compared cannot be chosen inside it.
            var candidates = useLambert2008
                ? _legacyContext.BuildingDetailsV2
                    .Where(building => boundingBox.Intersects(building.SysGeometryLambert2008))
                    .ToList()
                : _legacyContext.BuildingDetailsV2
                    .Where(building => boundingBox.Intersects(building.SysGeometry))
                    .ToList();

            var underlyingBuildings = candidates
                .Select(building => new { building.PersistentLocalId, Geometry = building.SysGeometryIn(matchingSrid) })
                .Where(building => building.Geometry is not null && matchingGeometry.Intersects(building.Geometry))
                .Select(building =>
                    new {
                        building.PersistentLocalId,
                        Overlap = CalculateOverlap(building.Geometry, matchingGeometry)
                    })
                .ToList();

            return underlyingBuildings
                .Where(building => building.Overlap >= 0.8 / underlyingBuildings.Count)
                .Select(building => building.PersistentLocalId);
        }

        private static Geometry ToMatchingCrs(Geometry geometry, int matchingSrid)
            => matchingSrid == SystemReferenceId.SridLambert2008
                ? geometry.IsLambert08() ? geometry : geometry.EnsureLambert08()
                : geometry.IsLambert72() ? geometry : geometry.EnsureLambert72();

        private static double CalculateOverlap(Geometry? buildingGeometry, Geometry parcel)
        {
            if (buildingGeometry is null)
            {
                return 0;
            }

            try
            {
                return buildingGeometry.Intersection(parcel).Area / buildingGeometry.Area;
            }
            catch (TopologyException topologyException)
            {
                // Consider parcels that Intersect, but fail with "found non-noded intersection" on calculating, to have an overlap value of 0
                if (topologyException.Message.Contains("found non-noded intersection", StringComparison.InvariantCultureIgnoreCase))
                    return 0;

                // any other TopologyException should be treated normally
                throw;
            }
        }
    }
}

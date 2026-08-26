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

        public BuildingMatching(
            LegacyContext legacyContext,
            Lambert2008ConversionCompletedToggle conversionCompleted)
        {
            _legacyContext = legacyContext;
            _conversionCompleted = conversionCompleted;
        }

        /// <summary>
        /// Finds the buildings under a parcel.
        ///
        /// The incoming parcel geometry is brought to the reference system matching is done in before
        /// anything is compared. Both layers below need that: SQL Server returns NULL rather than erroring
        /// when SRIDs disagree, and NTS ignores SRID entirely — it would compare a Lambert 72 polygon
        /// against a Lambert 2008 one, find them ~500 km apart, and return an empty intersection with no
        /// exception at all. See ADR 0006.
        ///
        /// This assumes <c>BuildingDetailV2.SysGeometry</c> is uniformly in that system. That is a property
        /// of Projections.Legacy, whose reference system ADR 0005 left undecided; ADR 0006 records the
        /// dependency.
        /// </summary>
        public IEnumerable<int> GetUnderlyingBuildings(Geometry parcelGeometry)
        {
            var matchingGeometry = ToMatchingCrs(parcelGeometry);
            var boundingBox = matchingGeometry.Factory.ToGeometry(matchingGeometry.EnvelopeInternal);

            var underlyingBuildings = _legacyContext
                .BuildingDetailsV2
                .Where(building => boundingBox.Intersects(building.SysGeometry))
                .ToList()
                .Where(building => matchingGeometry.Intersects(building.SysGeometry))
                .Select(building =>
                    new {
                        building.PersistentLocalId,
                        Overlap = CalculateOverlap(building.SysGeometry, matchingGeometry)
                    })
                .ToList();

            return underlyingBuildings
                .Where(building => building.Overlap >= 0.8 / underlyingBuildings.Count)
                .Select(building => building.PersistentLocalId);
        }

        private Geometry ToMatchingCrs(Geometry geometry)
            => _conversionCompleted.MatchingSrid == SystemReferenceId.SridLambert2008
                ? geometry.IsLambert08() ? geometry : geometry.EnsureLambert08(2)
                : geometry.IsLambert72() ? geometry : geometry.EnsureLambert72().RoundCoordinates(2);

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

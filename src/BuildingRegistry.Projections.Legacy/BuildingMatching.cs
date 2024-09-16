namespace BuildingRegistry.Projections.Legacy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class BuildingMatching : IBuildingMatching
    {
        private readonly LegacyContext _legacyContext;

        public BuildingMatching(LegacyContext legacyContext)
        {
            _legacyContext = legacyContext;
        }

        public IEnumerable<int> GetUnderlyingBuildings(Geometry parcelGeometry)
        {
            var boundingBox = parcelGeometry.Factory.ToGeometry(parcelGeometry.EnvelopeInternal);

            var underlyingBuildings = _legacyContext
                .BuildingDetailsV2
                .Where(building => boundingBox.Intersects(building.SysGeometry))
                .ToList()
                .Where(building => parcelGeometry.Intersects(building.SysGeometry))
                .Select(building =>
                    new {
                        building.PersistentLocalId,
                        Overlap = CalculateOverlap(building.SysGeometry, parcelGeometry)
                    })
                .ToList();

            return underlyingBuildings
                .Where(building => building.Overlap >= 0.8 / underlyingBuildings.Count)
                .Select(building => building.PersistentLocalId);
        }

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

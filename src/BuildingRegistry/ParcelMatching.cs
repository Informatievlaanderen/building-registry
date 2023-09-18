namespace BuildingRegistry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Building;
    using NetTopologySuite.Geometries;

    public class ParcelMatching : IParcelMatching
    {
        private readonly IParcels _parcels;

        public ParcelMatching(IParcels parcels)
        {
            _parcels = parcels;
        }

        public async Task<IEnumerable<ParcelData>> GetUnderlyingParcels(Geometry geometry)
        {
            var underlyingParcels = (await _parcels.GetUnderlyingParcelsUnderBoundingBox(geometry))
                .Where(parcel => geometry.Intersects(parcel.Geometry) && parcel.Status == "Realized")
                .Select(parcel =>
                    new
                    {
                        Overlap = CalculateOverlap(geometry, parcel.Geometry),
                        ParcelData = parcel
                    })
                .ToList();

            var overlappingParcels = underlyingParcels
                .Where(parcel => parcel.Overlap >= 0.8 / underlyingParcels.Count)
                .Select(x => x.ParcelData);

            return overlappingParcels;
        }

        private static double CalculateOverlap(Geometry building, Geometry parcel)
        {
            try
            {
                return building.Intersection(parcel).Area / building.Area;
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

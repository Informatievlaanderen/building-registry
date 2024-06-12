namespace BuildingRegistry.Api.Legacy.Infrastructure.ParcelMatching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Consumer.Read.Parcel;
    using Consumer.Read.Parcel.ParcelWithCount;
    using NetTopologySuite.Geometries;

    public class ParcelMatching : IParcelMatching
    {
        private readonly ConsumerParcelContext _context;

        public ParcelMatching(ConsumerParcelContext context)
        {
            _context = context;
        }

        public IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes)
        {
            var buildingGeometry = WKBReaderFactory.Create().Read(buildingGeometryBytes);
            var boundingBox = buildingGeometry.Factory.ToGeometry(buildingGeometry.EnvelopeInternal);

            var underlyingParcels = _context
                .ParcelConsumerItemsWithCount
                .Where(parcel => boundingBox.Intersects(parcel.Geometry))
                .ToList()
                .Where(parcel => buildingGeometry.Intersects(parcel.Geometry) && parcel.Status == ParcelStatus.Realized)
                .Select(parcel =>
                    new {
                        parcel.CaPaKey,
                        Overlap = CalculateOverlap(buildingGeometry, parcel.Geometry)
                    })
                .ToList();

            return underlyingParcels
                .Where(parcel => parcel.Overlap >= 0.8 / underlyingParcels.Count)
                .Select(parcel => parcel.CaPaKey);
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

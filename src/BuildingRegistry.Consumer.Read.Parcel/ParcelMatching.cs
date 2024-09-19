namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;
    using ParcelWithCount;

    public class ParcelMatching : IParcelMatching
    {
        private readonly ConsumerParcelContext _consumerParcelContext;

        public ParcelMatching(ConsumerParcelContext consumerParcelContext)
        {
            _consumerParcelContext = consumerParcelContext;
        }

        public IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes)
        {
            var buildingGeometry = WKBReaderFactory.Create().Read(buildingGeometryBytes);
            var boundingBox = buildingGeometry.Factory.ToGeometry(buildingGeometry.EnvelopeInternal);

            var underlyingParcels = _consumerParcelContext
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

namespace BuildingRegistry.Api.Oslo.Infrastructure.ParcelMatching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Consumer.Read.Parcel;
    using NetTopologySuite.Geometries;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetailV2;

    public class ParcelMatching : IParcelMatching
    {
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly LegacyContext _legacyContext;

        public ParcelMatching(ConsumerParcelContext consumerParcelContext, LegacyContext legacyContext)
        {
            _consumerParcelContext = consumerParcelContext;
            _legacyContext = legacyContext;
        }

        public IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometryBytes)
        {
            var buildingGeometry = WKBReaderFactory.Create().Read(buildingGeometryBytes);
            var boundingBox = buildingGeometry.Factory.ToGeometry(buildingGeometry.EnvelopeInternal);

            var underlyingParcels = _consumerParcelContext
                .ParcelConsumerItems
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

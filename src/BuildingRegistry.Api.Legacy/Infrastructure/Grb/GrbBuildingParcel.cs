namespace BuildingRegistry.Api.Legacy.Abstractions.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;
    using Wfs;

    public interface IGrbBuildingParcel
    {
        IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometry);
    }

    public class GrbBuildingParcel : IGrbBuildingParcel
    {
        private readonly IGrbWfsClient _wfsClient;

        public GrbBuildingParcel(IGrbWfsClient wfsClient) => _wfsClient = wfsClient;

        public IEnumerable<string> GetUnderlyingParcels(byte[] buildingGeometry)
        {
            var building = WKBReaderFactory.Create().Read(buildingGeometry);

            var parcelFeatures = _wfsClient.GetFeaturesInBoundingBox(GrbFeatureType.Parcel, building.EnvelopeInternal);
            var uniqueParcels = new Dictionary<string, Geometry>();
            foreach (var (parcelGeometry, parcelProperties) in parcelFeatures)
            {
                if (!uniqueParcels.ContainsKey(parcelProperties["CAPAKEY"]))
                    uniqueParcels.Add(parcelProperties["CAPAKEY"], parcelGeometry);
            }

            var intersectingParcels = uniqueParcels
                .Where(parcel => building.Intersects(parcel.Value))
                .Select(parcel =>
                    new {
                        parcel.Key,
                        Overlap = CalculateOverlap(building, parcel.Value)
                    })
                .ToList();

            return intersectingParcels
                .Where(parcel => parcel.Overlap >= 0.8 / intersectingParcels.Count)
                .Select(parcelPair => parcelPair.Key);
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

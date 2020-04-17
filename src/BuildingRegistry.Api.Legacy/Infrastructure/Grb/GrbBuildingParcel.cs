namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

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
                        Overlap = building.Intersection(parcel.Value).Area / building.Area
                    })
                .ToList();

            return intersectingParcels
                .Where(parcel => parcel.Overlap >= 0.8 / intersectingParcels.Count)
                .Select(parcelPair => parcelPair.Key);
        }
    }
}

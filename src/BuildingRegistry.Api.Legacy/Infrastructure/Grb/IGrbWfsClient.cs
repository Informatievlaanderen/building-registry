namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using GeoAPI.Geometries;

    public interface IGrbWfsClient
    {
        IEnumerable<Tuple<IGeometry, IReadOnlyDictionary<string, string>>> GetFeaturesInBoundingBox(GrbFeatureType featureType, Envelope boundingBox);
    }

    public enum GrbFeatureType
    {
        Parcel,
        Waterway
    }
}

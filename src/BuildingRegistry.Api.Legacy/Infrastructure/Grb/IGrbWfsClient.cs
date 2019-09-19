namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public interface IGrbWfsClient
    {
        IEnumerable<Tuple<Geometry, IReadOnlyDictionary<string, string>>> GetFeaturesInBoundingBox(GrbFeatureType featureType, Envelope boundingBox);
    }

    public enum GrbFeatureType
    {
        Parcel,
        Waterway
    }
}

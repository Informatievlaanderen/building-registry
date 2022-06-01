namespace BuildingRegistry.Api.Oslo.Abstractions.Infrastructure.Grb
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class GrbFeatureName
    {
        private static readonly ReadOnlyDictionary<GrbFeatureType, string> FeatureNames = new ReadOnlyDictionary<GrbFeatureType, string>(
            new Dictionary<GrbFeatureType, string>
            {
                { GrbFeatureType.Parcel, "ADP"},
                { GrbFeatureType.Waterway, "GRB_-_Wtz_-_watergang" }
            });

        private readonly string _name;

        private GrbFeatureName(string name)
            => _name = name;

        public static GrbFeatureName For(GrbFeatureType type)
            => FeatureNames.ContainsKey(type)
                ? new GrbFeatureName(FeatureNames[type])
                : throw new ArgumentException($"Name of GRB feature type [{Enum.GetName(typeof(GrbFeatureType), type)}] cannot be resolved.");

        public override string ToString()
            => _name;
    }
}

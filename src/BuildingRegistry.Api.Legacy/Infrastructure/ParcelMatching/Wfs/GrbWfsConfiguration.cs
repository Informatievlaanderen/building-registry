namespace BuildingRegistry.Api.Legacy.Infrastructure.ParcelMatching.Wfs
{
    using System;

    public class GrbWfsConfiguration
    {
        public string Url { get; }
        
        public GrbWfsConfiguration(string url)
            => Url = !string.IsNullOrWhiteSpace(url)
                ? url
                : throw new ArgumentNullException(nameof(url));
    }
}

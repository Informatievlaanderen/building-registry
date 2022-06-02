namespace BuildingRegistry.Api.Legacy.Abstractions.Infrastructure.Grb.Wfs
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

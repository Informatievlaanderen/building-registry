namespace BuildingRegistry.Api.Legacy.Infrastructure.Grb.Wfs
{
    using System;

    internal class GrbWfsConfiguration
    {
        public GrbWfsConfiguration(string url)
            => Url = !string.IsNullOrWhiteSpace(url)
                ? url
                : throw new ArgumentNullException(nameof(url));

        public string Url { get; }
    }
}

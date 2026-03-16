namespace BuildingRegistry.Tests
{
    using NetTopologySuite.IO;

    public static class WkbWriter
    {
        private static readonly WKBWriter Writer = new()
        {
            Strict = false,   // allow extended flavor
            HandleSRID = true // embed SRID
        };

        public static WKBWriter Instance => Writer;
    }
}

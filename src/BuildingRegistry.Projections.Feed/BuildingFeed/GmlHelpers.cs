namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;

    public static class GmlHelpers
    {
        public static Geometry ParseGeometry(string extendedWkbGeometryHex)
        {
            var reader = WKBReaderFactory.Create();
            return reader.Read(extendedWkbGeometryHex.ToByteArray());
        }
    }
}

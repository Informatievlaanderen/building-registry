namespace BuildingRegistry.Api.BackOffice.Abstractions.Building
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.IO;
    using NetTopologySuite.IO.GML2;
    using ExtendedWkbGeometry = BuildingRegistry.Building.ExtendedWkbGeometry;

    public static class GmlHelpers
    {
        private static readonly WKBWriter WkbWriter = new WKBWriter() { Strict = false, HandleSRID = true };

        public static GMLReader CreateGmlReader() =>
            new GMLReader(NtsGeometryFactory.CreateGeometryFactoryLambert72());

        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return ExtendedWkbGeometry.CreateEWkb(WkbWriter.Write(geometry));
        }
    }
}

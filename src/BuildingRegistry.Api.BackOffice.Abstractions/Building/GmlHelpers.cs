namespace BuildingRegistry.Api.BackOffice.Abstractions.Building
{
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO.GML2;
    using ExtendedWkbGeometry = BuildingRegistry.Building.ExtendedWkbGeometry;
    using GeometryFactory = BuildingRegistry.GeometryFactory;

    public static class GmlHelpers
    {
        public static GMLReader CreateGmlReader() =>
            new GMLReader(GeometryFactory.CreateGeometryFactory());

        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return ExtendedWkbGeometry.CreateEWkb(geometry.AsBinary());
        }
    }
}

namespace BuildingRegistry.Api.BackOffice.Abstractions.Building
{
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO.GML2;
    using ExtendedWkbGeometry = BuildingRegistry.Building.ExtendedWkbGeometry;

    public static class GmlHelpers
    {
        public static GMLReader CreateGmlReader() =>
            new GMLReader(
                new GeometryFactory(
                    new PrecisionModel(PrecisionModels.Floating),
                    ExtendedWkbGeometry.SridLambert72,
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY)));

        public static ExtendedWkbGeometry ToExtendedWkbGeometry(this string gml)
        {
            var gmlReader = CreateGmlReader();
            var geometry = gmlReader.Read(gml);

            geometry.SRID = ExtendedWkbGeometry.SridLambert72;

            return ExtendedWkbGeometry.CreateEWkb(geometry.AsBinary());
        }
    }
}

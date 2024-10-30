namespace BuildingRegistry
{
    using Building;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;

    // ReSharper disable once InconsistentNaming
    public static class WKBReaderFactory
    {
        public static WKBReader Create() => new WKBReader(GeometryFactory.CreateNtsGeometryServices());
    }

    public static class GeometryFactory
    {
        public static NtsGeometryServices CreateNtsGeometryServices() =>
            new NtsGeometryServices(
                new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                new PrecisionModel(PrecisionModels.Floating),
                ExtendedWkbGeometry.SridLambert72);

        public static NetTopologySuite.Geometries.GeometryFactory CreateGeometryFactory() => CreateNtsGeometryServices().CreateGeometryFactory();
    }
}

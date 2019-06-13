namespace BuildingRegistry
{
    using GeoAPI.Geometries;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using ValueObjects;

    public static class WKBReaderFactory
    {
        public static WKBReader Create() =>
            new WKBReader(
                new NtsGeometryServices(
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                    new PrecisionModel(PrecisionModels.Floating),
                    WkbGeometry.SridLambert72));
    }
}

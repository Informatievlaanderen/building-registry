namespace BuildingRegistry.Tests.Autofixture
{
    using AutoFixture;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using ValueObjects;

    public static class GeometryHelper
    {
        public static Geometry ValidPolygon =>
            new WKTReader().Read(
                "POLYGON ((141298.83027724177 185196.03552261367, 141294.79827723652 185190.20384261012, 141296.80672523379 185188.7793306075, 141295.2384692356 185186.52896260843, 141296.27578123659 185185.72653060779, 141294.88224523515 185183.81600260362, 141296.85165324062 185182.33645060286, 141298.27155724168 185184.30649860576, 141298.47520523518 185184.18451460451, 141304.05254924297 185192.11923461035, 141298.83027724177 185196.03552261367))");

        public static Geometry ValidPolygonWithNoValidPoints =>
            new WKTReader().Read(
                "POLYGON ((30 10, 10 20, 20 40, 40 40, 30 10))");

        public static Geometry ValidPointInPolygon =>
            new WKTReader().Read("POINT (141299 185188)");

        public static Geometry OtherValidPointInPolygon =>
            new WKTReader().Read("POINT (141298 185187)");

        public static Geometry PointNotInPolygon =>
            new WKTReader().Read("POINT (1 1)");

        private static readonly WKBWriter WkbWriter = new WKBWriter { Strict = false, HandleSRID = true };

        public static ExtendedWkbGeometry CreateEwkbFrom(WkbGeometry wkbGeometry)
        {
            var reader = new WKBReader();
            var geometry = reader.Read(wkbGeometry);
            geometry.SRID = WkbGeometry.SridLambert72;
            return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
        }

        public static WkbGeometry CreateFromWkt(string wkt)
        {
            var geometry = new WKTReader { DefaultSRID = WkbGeometry.SridLambert72 }.Read(wkt);
            return new WkbGeometry(WkbWriter.Write(geometry));
        }
    }

    public class WithValidPolygon : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())));
        }
    }

    public class WithValidPolygonOutsideValidPointBoundary : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())));
        }
    }

    public class WithValidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary())));
        }
    }

    public class WithOtherValidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary())));
        }
    }

    public class WithInvalidPoint : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customize<WkbGeometry>(c => c.FromFactory(() => new WkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary())));
        }
    }
}

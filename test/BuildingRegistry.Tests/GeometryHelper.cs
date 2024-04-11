namespace BuildingRegistry.Tests
{
    using BuildingRegistry.Legacy;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public static class GeometryHelper
    {
        public static Geometry ValidPolygon =>
            new WKTReader().Read(
                "POLYGON ((141298.83027724177 185196.03552261367, 141294.79827723652 185190.20384261012, 141296.80672523379 185188.7793306075, 141295.2384692356 185186.52896260843, 141296.27578123659 185185.72653060779, 141294.88224523515 185183.81600260362, 141296.85165324062 185182.33645060286, 141298.27155724168 185184.30649860576, 141298.47520523518 185184.18451460451, 141304.05254924297 185192.11923461035, 141298.83027724177 185196.03552261367))");

        // Subtracted 1 from every point in the above polygon
        public static Geometry SecondValidPolygon =>
            new WKTReader().Read(
                "POLYGON ((141297.83027724177 185195.03552261367, 141293.79827723652 185189.20384261012, 141295.80672523379 185187.7793306075, 141294.2384692356 185185.52896260843, 141295.27578123659 185184.72653060779, 141293.88224523515 185182.81600260362, 141295.85165324062 185181.33645060286, 141297.27155724168 185183.30649860576, 141297.47520523518 185183.18451460451, 141303.05254924297 185191.11923461035, 141297.83027724177 185195.03552261367))");

        public static Geometry ValidPolygonWithNoValidPoints =>
            new WKTReader().Read(
                "POLYGON ((30 10, 10 20, 20 40, 40 40, 30 10))");

        public static Geometry ValidPointInPolygon =>
            new WKTReader().Read("POINT (141299 185188)");

        public static Geometry OtherValidPointInPolygon =>
            new WKTReader().Read("POINT (141298 185187)");

        public static Geometry PointNotInPolygon =>
            new WKTReader().Read("POINT (1 1)");

        public static Geometry SelfTouchingPolygon =
            new WKTReader().Read(
                "POLYGON ((30359.924344554543 197007.54170677811, 30359.446008555591 197010.21338678151, 30371.943992562592 197013.23297078162, 30373.701176568866 197006.42113077641, 30363.939512558281 197004.00340277702, 30364.205112561584 197002.85997877643, 30357.719608552754 197001.36161077395, 30356.638264551759 197006.90023477748, 30359.924344554543 197007.54170677811, 30360.468344554305 197004.48564277589, 30362.562808558345 197004.85844277591, 30362.018680557609 197007.91457077861, 30359.924344554543 197007.54170677811))");

        public static string selfTouchingGml =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>30359.924344554543 197007.54170677811 30359.446008555591 197010.21338678151 30371.943992562592 197013.23297078162 30373.701176568866 197006.42113077641 30363.939512558281 197004.00340277702 30364.205112561584 197002.85997877643 30357.719608552754 197001.36161077395 30356.638264551759 197006.90023477748 30359.924344554543 197007.54170677811 30360.468344554305 197004.48564277589 30362.562808558345 197004.85844277591 30362.018680557609 197007.91457077861 30359.924344554543 197007.54170677811</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";


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

        public static Geometry CreateGeometryFromWkt(string wkt)
        {
            return new WKTReader { DefaultSRID = WkbGeometry.SridLambert72 }.Read(wkt);
        }
    }
}

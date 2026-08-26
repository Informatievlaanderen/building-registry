namespace BuildingRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using BuildingRegistry.Legacy;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public static class GeometryHelper
    {
        public static Geometry ValidPolygon =>
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read(
                "POLYGON ((141298.83027724177 185196.03552261367, 141294.79827723652 185190.20384261012, 141296.80672523379 185188.7793306075, 141295.2384692356 185186.52896260843, 141296.27578123659 185185.72653060779, 141294.88224523515 185183.81600260362, 141296.85165324062 185182.33645060286, 141298.27155724168 185184.30649860576, 141298.47520523518 185184.18451460451, 141304.05254924297 185192.11923461035, 141298.83027724177 185196.03552261367))");

        // Subtracted 1 from every point in the above polygon
        public static Geometry SecondValidPolygon =>
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read(
                "POLYGON ((141297.83027724177 185195.03552261367, 141293.79827723652 185189.20384261012, 141295.80672523379 185187.7793306075, 141294.2384692356 185185.52896260843, 141295.27578123659 185184.72653060779, 141293.88224523515 185182.81600260362, 141295.85165324062 185181.33645060286, 141297.27155724168 185183.30649860576, 141297.47520523518 185183.18451460451, 141303.05254924297 185191.11923461035, 141297.83027724177 185195.03552261367))");

        public static Geometry ValidPolygonWithNoValidPoints =>
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read(
                "POLYGON ((30 10, 10 20, 20 40, 40 40, 30 10))");

        public static Geometry ValidPointInPolygon =>
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read("POINT (141299 185188)");

        public static Geometry OtherValidPointInPolygon =>
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read("POINT (141298 185187)");

        public static Geometry PointNotInPolygon =>
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read("POINT (1 1)");

        public static Geometry SelfTouchingPolygon =
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read(
                "POLYGON ((30359.924344554543 197007.54170677811, 30359.446008555591 197010.21338678151, 30371.943992562592 197013.23297078162, 30373.701176568866 197006.42113077641, 30363.939512558281 197004.00340277702, 30364.205112561584 197002.85997877643, 30357.719608552754 197001.36161077395, 30356.638264551759 197006.90023477748, 30359.924344554543 197007.54170677811, 30360.468344554305 197004.48564277589, 30362.562808558345 197004.85844277591, 30362.018680557609 197007.91457077861, 30359.924344554543 197007.54170677811))");

        /// <summary>
        /// <see cref="ValidPolygon"/> as GML, in the https srsName form the swagger examples use.
        /// </summary>
        public const string GmlPolygonGeometry =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>141298.83027724177 185196.03552261367 141294.79827723652 185190.20384261012 141296.80672523379 185188.77933060750 141295.23846923560 185186.52896260843 141296.27578123659 185185.72653060779 141294.88224523515 185183.81600260362 141296.85165324062 185182.33645060286 141298.27155724168 185184.30649860576 141298.47520523518 185184.18451460451 141304.05254924297 185192.11923461035 141298.83027724177 185196.03552261367</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        /// <summary>
        /// <see cref="GmlPolygonGeometry"/> expressed in Lambert 2008 (EPSG 3812).
        /// </summary>
        public const string GmlPolygonGeometryLambert2008 =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>641296.80075767275 685195.39869418368 641292.76950100798 685189.56657716632 641294.77810088242 685188.14230902866 641293.21013184427 685185.89177094586 641294.24753033393 685185.08946529962 641292.85423856950 685183.17878562119 641294.82380548702 685181.69947324693 641296.24345780967 685183.66967565194 641296.44711849571 685183.54771624226 641302.02345018531 685191.48304155469 641296.80075767275 685195.39869418368</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        /// <summary>
        /// <see cref="GmlPolygonGeometryLambert2008"/> converted back to Lambert 72 by the geometry normalizer:
        /// an http srsName per <c>SystemReferenceId.SrsNameLambert72</c>, and coordinates that differ from
        /// <see cref="GmlPolygonGeometry"/> in the 11th decimal — the round trip is not bit-exact at that precision.
        /// Those last decimals also differ per operating system, so compare against this with
        /// <see cref="GmlAssertions.ShouldBeEquivalentGml"/> rather than as a string.
        /// </summary>
        public const string NormalizedGmlPolygonGeometry =
            "<gml:Polygon srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>141298.83027724162 185196.03552261207 141294.79827723664 185190.20384261021 141296.80672523385 185188.77933060596 141295.23846923554 185186.52896260860 141296.27578123665 185185.72653060872 141294.88224523523 185183.81600260572 141296.85165324068 185182.33645060271 141298.27155724179 185184.30649860427 141298.47520523507 185184.18451460422 141304.05254924300 185192.11923460962 141298.83027724162 185196.03552261207</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        /// <summary>
        /// <see cref="GmlPolygonGeometry"/> converted to Lambert 2008 by the geometry normalizer: an http srsName
        /// per <c>SystemReferenceId.SrsNameLambert2008</c>. Its last decimals differ per operating system, so compare
        /// against this with <see cref="GmlAssertions.ShouldBeEquivalentGml"/> rather than as a string.
        /// </summary>
        public const string NormalizedGmlPolygonGeometryLambert2008 =
            "<gml:Polygon srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>641296.80075767275 685195.39869418368 641292.76950100798 685189.56657716632 641294.77810088242 685188.14230902866 641293.21013184427 685185.89177094586 641294.24753033393 685185.08946529962 641292.85423856950 685183.17878562119 641294.82380548702 685181.69947324693 641296.24345780967 685183.66967565194 641296.44711849571 685183.54771624226 641302.02345018531 685191.48304155469 641296.80075767275 685195.39869418368</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        /// <summary>
        /// <see cref="NormalizedGmlPolygonGeometry"/> after <c>ToCleanPolygon()</c>, which the two outline actions run
        /// on the normalized polygon: same coordinates, back to the https srsName. Converting to Lambert 2008 needs no
        /// counterpart — cleaning <see cref="NormalizedGmlPolygonGeometryLambert2008"/> yields
        /// <see cref="GmlPolygonGeometryLambert2008"/>.
        /// </summary>
        public const string CleanedNormalizedGmlPolygonGeometry =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>141298.83027724162 185196.03552261207 141294.79827723664 185190.20384261021 141296.80672523385 185188.77933060596 141295.23846923554 185186.52896260860 141296.27578123665 185185.72653060872 141294.88224523523 185183.81600260572 141296.85165324068 185182.33645060271 141298.27155724179 185184.30649860427 141298.47520523507 185184.18451460422 141304.05254924300 185192.11923460962 141298.83027724162 185196.03552261207</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        /// <summary>
        /// <see cref="ValidPointInPolygon"/> as GML, in the https srsName form the swagger examples use.
        /// </summary>
        public const string GmlPointGeometry =
            "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>141299.00 185188.00</gml:pos></gml:Point>";

        /// <summary>
        /// <see cref="GmlPointGeometry"/> expressed in Lambert 2008 (EPSG 3812).
        /// </summary>
        public const string GmlPointGeometryLambert2008 =
            "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>641296.97 685187.36</gml:pos></gml:Point>";

        /// <summary>
        /// <see cref="GmlPointGeometry"/> as re-serialized by the geometry normalizer: an http srsName,
        /// per <c>SystemReferenceId.SrsNameLambert72</c>. A 72 → 08 → 72 round trip is exact at point precision.
        /// </summary>
        public const string NormalizedGmlPointGeometry =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>141299.00 185188.00</gml:pos></gml:Point>";

        /// <summary>
        /// <see cref="GmlPointGeometryLambert2008"/> as re-serialized by the geometry normalizer: an http srsName,
        /// per <c>SystemReferenceId.SrsNameLambert2008</c>.
        /// </summary>
        public const string NormalizedGmlPointGeometryLambert2008 =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>641296.97 685187.36</gml:pos></gml:Point>";

        public static string selfTouchingGml =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>30359.924344554543 197007.54170677811 30359.446008555591 197010.21338678151 30371.943992562592 197013.23297078162 30373.701176568866 197006.42113077641 30363.939512558281 197004.00340277702 30364.205112561584 197002.85997877643 30357.719608552754 197001.36161077395 30356.638264551759 197006.90023477748 30359.924344554543 197007.54170677811 30360.468344554305 197004.48564277589 30362.562808558345 197004.85844277591 30362.018680557609 197007.91457077861 30359.924344554543 197007.54170677811</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        public static Geometry TooSmallPolygon =
            new WKTReader(NtsGeometryFactory.CreateGeometryFactoryLambert72()).Read(
                "POLYGON ((0 0, 0.5 0, 0.5 0.5, 0 0.5, 0 0))");

        public static ExtendedWkbGeometry CreateEwkbFrom(WkbGeometry wkbGeometry)
        {
            var reader = new WKBReader();
            var geometry = reader.Read(wkbGeometry);
            geometry.SRID = WkbGeometry.SridLambert72;
            return new ExtendedWkbGeometry(WkbWriter.Instance.Write(geometry));
        }

        public static WkbGeometry CreateFromWkt(string wkt)
        {
            var geometry = new WKTReader { DefaultSRID = WkbGeometry.SridLambert72 }.Read(wkt);
            return new WkbGeometry(WkbWriter.Instance.Write(geometry));
        }

        /// <summary>
        /// EWKB for <paramref name="wkt"/> in <paramref name="srid"/>, so a test can hand a projection an
        /// event geometry in either reference system. The SRID is written into the bytes, which is what
        /// <c>WKBReaderFactory.CreateForEwkb</c> reads it back from. See ADR 0005.
        /// </summary>
        public static BuildingRegistry.Building.ExtendedWkbGeometry CreateEwkbFromWkt(string wkt, int srid)
        {
            var geometry = new WKTReader { DefaultSRID = srid }.Read(wkt);
            geometry.SRID = srid;

            return new BuildingRegistry.Building.ExtendedWkbGeometry(WkbWriter.Instance.Write(geometry));
        }

        public static Geometry CreateGeometryFromWkt(string wkt)
        {
            return new WKTReader { DefaultSRID = WkbGeometry.SridLambert72 }.Read(wkt);
        }
    }
}

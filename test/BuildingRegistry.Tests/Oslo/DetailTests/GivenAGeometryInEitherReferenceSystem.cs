namespace BuildingRegistry.Tests.Oslo.DetailTests
{
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Api.Oslo.Building.V2.Detail;
    using Api.Oslo.BuildingUnit.V2.Detail;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Building;
    using FluentAssertions;
    using Xunit;

    /// <summary>
    /// Version 2's GML carries an <c>srsName</c> hardcoded on EPSG 31370, so the response is only honest
    /// while everything reaching it is Lambert 72. Once the event store holds Lambert 2008 that stops being
    /// true of the stored geometry — <c>BuildingDetailV2Projections</c> keeps the payload verbatim — so the
    /// handler is what has to bring it back. Getting this wrong is silent: the label still says 31370 and
    /// the coordinates are ~500 km away. See ADR 0008.
    /// </summary>
    public class GivenAGeometryInEitherReferenceSystem
    {
        private const string Lambert72Polygon =
            "POLYGON ((141298.83 185196.04, 141294.8 185190.2, 141296.81 185188.78, 141295.24 185186.53, 141296.28 185185.73, 141294.88 185183.82, 141296.85 185182.34, 141298.27 185184.31, 141298.48 185184.18, 141304.05 185192.12, 141298.83 185196.04))";

        private const string Lambert2008Polygon =
            "POLYGON ((641296.8 685195.4, 641292.77 685189.56, 641294.78 685188.14, 641293.21 685185.89, 641294.25 685185.09, 641292.85 685183.18, 641294.82 685181.7, 641296.24 685183.67, 641296.45 685183.54, 641302.02 685191.48, 641296.8 685195.4))";

        private const string Lambert72Point = "POINT (141299 185188)";
        private const string Lambert2008Point = "POINT (641296.97 685187.36)";

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Polygon)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Polygon)]
        public void ThenTheBuildingIsAnsweredInLambert72(int srid, string wkt)
        {
            var polygon = BuildingDetailHandlerV2.GetBuildingPolygon(
                GeometryHelper.CreateEwkbFromWkt(wkt, srid).ToByteArray()!,
                BuildingGeometryMethod.Outlined);

            var gml = polygon.Geometry.Gml;

            gml.Should().Contain("EPSG/0/31370");

            // The label is not the assertion — it is hardcoded, and that is the whole problem. The
            // coordinates are. Approximately, not exactly: an outline that arrived as Lambert 2008 is
            // rendered at the precision the transform produces rather than rounded, so it lands within
            // the transform's accuracy of one that arrived as Lambert 72.
            var first = FirstCoordinate(gml, "posList");

            first.x.Should().BeApproximately(141298.83, 0.01);
            first.y.Should().BeApproximately(185196.04, 0.01);
        }

        [Theory]
        [InlineData(SystemReferenceId.SridLambert72, Lambert72Point)]
        [InlineData(SystemReferenceId.SridLambert2008, Lambert2008Point)]
        public void ThenTheBuildingUnitIsAnsweredInLambert72(int srid, string wkt)
        {
            var position = BuildingUnitDetailHandlerV2.GetBuildingUnitPoint(
                GeometryHelper.CreateEwkbFromWkt(wkt, srid).ToByteArray()!,
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator);

            var gml = position.Geometry.Gml;

            gml.Should().Contain("EPSG/0/31370");

            // A transformed position is rounded to the centimetre, so it renders identically to the same
            // position while the event store still held Lambert 72 — exactly, not approximately.
            gml.Should().Contain("<gml:pos>141299.00 185188.00</gml:pos>");
        }

        private static (double x, double y) FirstCoordinate(string gml, string element)
        {
            var match = Regex.Match(gml, $@"<gml:{element}>(?<x>[\d.]+) (?<y>[\d.]+)");

            match.Success.Should().BeTrue();

            return (
                double.Parse(match.Groups["x"].Value, CultureInfo.InvariantCulture),
                double.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture));
        }
    }
}

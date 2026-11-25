namespace BuildingRegistry.Tests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    /// <summary>
    /// The position a building unit with geometry method DerivedFromObject takes. It is a building unit
    /// position like any other, so it is held at centimetre precision — unlike the building geometry it is
    /// computed from, whose vertices keep every decimal they came with. See ADR 0007.
    /// </summary>
    public class BuildingGeometryCenterTests
    {
        private static BuildingGeometry Outlined(Geometry polygon) =>
            new BuildingGeometry(
                ExtendedWkbGeometry.Create(polygon),
                BuildingGeometryMethod.Outlined);

        private static Point Center(Geometry polygon)
        {
            var ewkb = Outlined(polygon).Center.ToByteArray();
            return (Point)WKBReaderFactory.CreateForEwkb(ewkb).Read(ewkb);
        }

        /// <summary>
        /// The regression this exists for: a centroid carries as many decimals as the outline it comes from,
        /// so a derived position used to be the one kind of position the event store held at full precision —
        /// including the one the Lambert 2008 transformation writes, which rounds a unit's own position in
        /// the line above and inherited this one unrounded.
        /// </summary>
        [Fact]
        public void ThenTheCenterIsRoundedToCentimetres()
        {
            var center = Center(GeometryHelper.ValidPolygon);

            center.X.Should().Be(Round(center.X));
            center.Y.Should().Be(Round(center.Y));
        }

        [Fact]
        public void ThenTheCenterKeepsTheReferenceSystemOfTheBuildingGeometry()
        {
            Center(GeometryHelper.ValidPolygon).SRID.Should().Be(SystemReferenceId.SridLambert72);
            Center(GeometryHelper.ValidPolygonLambert2008).SRID.Should().Be(SystemReferenceId.SridLambert2008);
        }

        /// <summary>
        /// Reading it twice has to give the same bytes: the aggregate compares a stored position against a
        /// freshly computed centre to decide whether it changed, so a centre that rounded differently per
        /// call would apply an event every time.
        /// </summary>
        [Fact]
        public void ThenTheCenterIsStable()
        {
            var geometry = Outlined(GeometryHelper.ValidPolygon);

            geometry.Center.Should().Be(geometry.Center);
        }

        /// <summary>The building geometry itself is untouched — only the position taken from it is rounded.</summary>
        [Fact]
        public void ThenTheBuildingGeometryIsNotRounded()
        {
            var geometry = Outlined(GeometryHelper.ValidPolygon);

            _ = geometry.Center;

            geometry.GetGeometry().Coordinates[0].X.Should().Be(141298.83027724177);
        }

        /// <summary>
        /// A square with a long, low strip along its base pulls the centroid to x = 150010.00606, just inside
        /// the square's right edge at x = 150010.008, above the strip. Rounded to centimetres that is
        /// 150010.01 - outside the building, in the notch above the strip.
        /// </summary>
        private static readonly Polygon ConcaveBuildingWithCentroidNearTheEdge = new GeometryFactory(
                new PrecisionModel(), SystemReferenceId.SridLambert72)
            .CreatePolygon(new[]
            {
                new Coordinate(150000, 200000),
                new Coordinate(150041.648, 200000),
                new Coordinate(150041.648, 200001),
                new Coordinate(150010.008, 200001),
                new Coordinate(150010.008, 200010),
                new Coordinate(150000, 200010),
                new Coordinate(150000, 200000),
            });

        [Fact]
        public void GivenACentroidThatRoundsOutsideTheBuilding_ThenTheCenterIsStillInside()
        {
            var centroid = ConcaveBuildingWithCentroidNearTheEdge.Centroid;
            centroid.Within(ConcaveBuildingWithCentroidNearTheEdge).Should().BeTrue();
            ConcaveBuildingWithCentroidNearTheEdge.Factory
                .CreatePoint(new Coordinate(Round(centroid.X), Round(centroid.Y)))
                .Within(ConcaveBuildingWithCentroidNearTheEdge).Should().BeFalse("the test geometry has to reproduce the issue");

            var geometry = Outlined(ConcaveBuildingWithCentroidNearTheEdge);
            var center = Center(ConcaveBuildingWithCentroidNearTheEdge);

            geometry.Contains(geometry.Center).Should().BeTrue();
            center.X.Should().Be(150010.0);
            center.Y.Should().Be(Round(centroid.Y));
        }

        /// <summary>
        /// A real building after the Lambert 2008 transformation: its centroid lies 0.5 mm inside the edge of
        /// the notch at its north-east side, and rounded to 744478.27 698227.85 it lies 0.9 mm outside it - the
        /// position its derived building unit was given.
        /// </summary>
        [Fact]
        public void GivenARealBuildingWhoseRoundedCentroidLiesOutsideIt_ThenTheCenterIsTheNearestGridPointInside()
        {
            var polygon = new NetTopologySuite.IO.WKTReader(
                    new NetTopologySuite.NtsGeometryServices(new PrecisionModel(), SystemReferenceId.SridLambert2008))
                .Read("POLYGON((744480.1184530781 698229.8643112164,744479.9440770152 698237.9898144752,744472.9558250407 698237.9973120121,744472.9069092406 698223.7407438857,744478.4328983055 698223.7216822859,744478.4125888565 698217.8035041029,744483.4127542111 698217.8060764885,744483.8866076873 698217.8063242519,744483.7962069352 698219.2044500075,744485.3499138679 698219.3161202399,744484.3534088014 698226.7939782962,744483.3100937849 698226.7290878696,744483.2841918147 698227.1289546471,744478.2805655755 698227.1690695966,744478.2367449788 698229.777818243,744480.1184530781 698229.8643112164))");
            polygon.Factory.CreatePoint(new Coordinate(744478.27, 698227.85))
                .Within(polygon).Should().BeFalse("this is the position the building unit was given");

            var geometry = Outlined(polygon);
            var center = Center(polygon);

            geometry.Contains(geometry.Center).Should().BeTrue();
            center.X.Should().Be(744478.26);
            center.Y.Should().Be(698227.85);
            center.SRID.Should().Be(SystemReferenceId.SridLambert2008);
        }

        private static double Round(double value) =>
            System.Math.Round(value, GeometryReferenceSystem.PositionRoundingPrecision);
    }
}

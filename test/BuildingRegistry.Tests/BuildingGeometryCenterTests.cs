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

        private static double Round(double value) =>
            System.Math.Round(value, GeometryReferenceSystem.PositionRoundingPrecision);
    }
}

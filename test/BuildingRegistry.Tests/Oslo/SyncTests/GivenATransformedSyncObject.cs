namespace BuildingRegistry.Tests.Oslo.SyncTests
{
    using Api.BackOffice.Abstractions.Building;
    using Api.Oslo.Building.V2.Sync;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using FluentAssertions;
    using Xunit;

    /// <summary>
    /// The syndication object carries both shapes: the building's outline and, inside it, each unit's
    /// position. They go through one <see cref="SyncGeometry"/> but not one rule — a transformed position is
    /// rounded and a transformed outline is not — so a single shared code path would silently apply whichever
    /// rule it happened to hold to both. See ADR 0004.
    /// </summary>
    public class GivenATransformedSyncObject
    {
        private static byte[] ToExtendedWkb(string gml) => WkbWriter.Instance.Write(gml.ReadGeometry());

        /// <summary>
        /// Rounding a polygon moves every vertex and so its area, so the transform is taken at the precision
        /// it produces. Visible in the feed: the posList is rendered at full precision.
        /// </summary>
        [Fact]
        public void WhenTheOutlineIsTransformed_ThenItIsNotRounded()
        {
            var polygon = BuildingHelpers.GetBuildingPolygon(
                ToExtendedWkb(GeometryHelper.GmlPolygonGeometry),
                SystemReferenceId.SridLambert2008);

            var x = polygon.JsonPolygon.Coordinates[0][0][0];

            x.Should().NotBe(Round(x), "a transformed outline keeps the precision the transform produces");
            x.Should().BeApproximately(641296.80075767275, 0.01);
        }

        /// <summary>
        /// A position is a point: rounding moves it by at most half a centimetre and nothing downstream
        /// measures it. That is the address registry's case, and it is kept here.
        /// </summary>
        [Fact]
        public void WhenThePositionIsTransformed_ThenItIsRounded()
        {
            var point = BuildingUnitHelpers.GetBuildingUnitPoint(
                ToExtendedWkb(GeometryHelper.GmlPointGeometry),
                SystemReferenceId.SridLambert2008);

            var x = point.JsonPoint.Coordinates[0];

            x.Should().Be(Round(x), "a transformed position is rounded to the centimetre");
            x.Should().BeApproximately(641296.97, 0.01);
        }

        /// <summary>Neither is touched when it is already in the requested system.</summary>
        [Fact]
        public void WhenNothingHasToMove_ThenBothArePassedThrough()
        {
            var polygon = BuildingHelpers.GetBuildingPolygon(
                ToExtendedWkb(GeometryHelper.GmlPolygonGeometry),
                SystemReferenceId.SridLambert72);
            var point = BuildingUnitHelpers.GetBuildingUnitPoint(
                ToExtendedWkb(GeometryHelper.GmlPointGeometry),
                SystemReferenceId.SridLambert72);

            polygon.JsonPolygon.Coordinates[0][0][0].Should().Be(141298.83027724177);
            point.JsonPoint.Coordinates[0].Should().Be(141299.00);
        }

        private static double Round(double value) => System.Math.Round(value, 2);
    }
}

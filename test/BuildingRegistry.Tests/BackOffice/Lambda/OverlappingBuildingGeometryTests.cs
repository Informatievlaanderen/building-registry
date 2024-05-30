namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class OverlappingBuildingGeometryTests
    {
        private readonly FakeBuildingGeometryContext _buildingGeometryContext;

        public OverlappingBuildingGeometryTests()
        {
            _buildingGeometryContext = new FakeBuildingGeometryContextFactory().CreateDbContext([]);
        }

        [Fact]
        public void WithBuildingOverlappingOnlyOnBoundingBox_ShouldReturnNone()
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");
            var buildingGeometryOutsideOfBuildingGeometryButInsideBoundingBox = CreateGeometry("100 200 101 200 101 199 100 199 100 200");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(
                    1,
                    BuildingStatus.Planned,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometryOutsideOfBuildingGeometryButInsideBoundingBox,
                    false));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        [InlineData("Realized")]
        public void WithActiveBuildingHasOverlappingGeometry_ShouldReturnSingle(string status)
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(
                    1,
                    BuildingStatus.Parse(status),
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry,
                    false));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().ContainSingle();
        }

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithBuildingHasOverlappingGeometryButStatusIsInActive_ShouldReturnNothing(string status)
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(
                    1,
                    BuildingStatus.Parse(status),
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry,
                    false));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        [InlineData("Realized")]
        public void WithRemovedBuildingHasOverlappingGeometry_ShouldReturnNothing(string status)
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(
                    1,
                    BuildingStatus.Parse(status),
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry,
                    true));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().BeEmpty();
        }

        [Fact]
        public void WithBuildingOverlappingGeometryAndIdenticalId_ShouldReturnNothing()
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(
                    1,
                    BuildingStatus.Planned,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry,
                    false));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(1),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().BeEmpty();
        }

        [Fact]
        public void WithOverlappingBuilding_ShouldReturnNothing()
        {
            var polygon1 = CreateGeometry("150515.51954216015 193984.87173669986 150514.00391716015 193976.65298669986 150510.30950881125 193977.33427493152 150509.72481561470 193974.16367058750 150500.24419151392 193975.91199860617 150501.16168397170 193980.88726698520 150506.17659092674 193979.96246475200 150507.35941666557 193986.37655071693 150515.51954216015 193984.87173669986");
            var polygon2 = CreateGeometry("150527.43725961447 193983.04583268613 150515.52737160772 193984.85690468922 150513.92218761146 193976.26362468302 150513.68020360917 193974.55764068291 150519.21812361479 193973.77197667956 150518.91988360882 193971.80333667994 150522.89082761109 193971.20173668116 150526.00993161649 193973.88960868120 150527.43725961447 193983.04583268613");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(
                    2,
                    BuildingStatus.Realized,
                    BuildingGeometryMethod.MeasuredByGrb,
                    polygon2,
                    false));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(1),
                ExtendedWkbGeometry.CreateEWkb(polygon1.AsBinary())!);

            result.Should().BeEmpty();
        }

        private static Geometry CreateGeometry(string coordinates)
            => GmlHelpers.CreateGmlReader().Read(
                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                "<gml:exterior>" +
                "<gml:LinearRing>" +
                "<gml:posList>" +
                coordinates +
                "</gml:posList>" +
                "</gml:LinearRing>" +
                "</gml:exterior>" +
                "</gml:Polygon>");
    }
}

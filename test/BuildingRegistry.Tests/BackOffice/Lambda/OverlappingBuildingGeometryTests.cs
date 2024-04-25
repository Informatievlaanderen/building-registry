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

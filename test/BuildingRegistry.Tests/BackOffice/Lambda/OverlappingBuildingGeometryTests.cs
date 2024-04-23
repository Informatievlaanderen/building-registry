namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Tests.BackOffice;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class OverlappingBuildingGeometryTests
    {
        private readonly FakeBuildingGeometryContext _buildingGeometryContext;

        public OverlappingBuildingGeometryTests()
        {
            _buildingGeometryContext = new FakeBuildingGeometryContextFactory()
                .CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public void WithBuildingOverlappingOnlyOnBoundingBox_ShouldReturnNone()
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");
            var buildingGeometryOutsideOfBuildingGeometryButInsideBoundingBox = CreateGeometry("100 200 101 200 101 199 100 199 100 200");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometryOutsideOfBuildingGeometryButInsideBoundingBox));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().BeEmpty();
        }

        [Fact]
        public void WithBuildingOverlappingGeometry_ShouldReturnSingle()
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry));

            _buildingGeometryContext.SaveChanges();

            var result = _buildingGeometryContext.GetOverlappingBuildings(
                new BuildingPersistentLocalId(2),
                ExtendedWkbGeometry.CreateEWkb(buildingGeometry.AsBinary())!);

            result.Should().ContainSingle();
        }

        [Fact]
        public void WithBuildingOverlappingGeometryAndIdenticalId_ShouldReturnNone()
        {
            var buildingGeometry = CreateGeometry("100 100 150 200 200 200 200 100 100 100");

            _buildingGeometryContext.BuildingGeometries
                .Add(new BuildingGeometryData(1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry));

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

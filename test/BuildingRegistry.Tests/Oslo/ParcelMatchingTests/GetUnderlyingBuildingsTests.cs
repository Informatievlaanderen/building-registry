namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System.Linq;
    using Api.BackOffice.Abstractions.Building;
    using BackOffice;
    using Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetailV2;
    using Xunit;

    public class GetUnderlyingBuildingsTests
    {
        private readonly FakeLegacyContext _legacyContext;

        public GetUnderlyingBuildingsTests()
        {
            _legacyContext = new FakeLegacyContextFactory()
                .CreateDbContext([]);
        }

        [Fact]
        public void WithBuildingOverlapping100Percent_ThenReturnsTheUnderlyingBuilding()
        {
            var buildingGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var parcelGeometry100PercentOverlap = buildingGeometry;

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry.AsBinary(),
                    buildingGeometry,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext);

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry100PercentOverlap);

            result.Should().ContainSingle();
        }

        [Fact]
        public void WithBuildingLessThan80PercentOverlap_ThenReturnsNothing()
        {
            var buildingGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var parcelGeometry = CreateGeometry("140 100 140 200 240 200 240 100 140 100");

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry.AsBinary(),
                    buildingGeometry,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext);

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry);

            result.Should().BeEmpty();
        }

        [Fact]
        public void With2BuildingsAbove40PercentOverlap_ThenReturnsThe2Buildings()
        {
            var buildingGeometry50PercentOverlap = CreateGeometry("50 100 50 200 140 200 140 100 50 100");
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry50PercentOverlap.AsBinary(),
                    buildingGeometry50PercentOverlap,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    2,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingGeometry50PercentOverlap.AsBinary(),
                    buildingGeometry50PercentOverlap,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext);

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry);

            result.Count().Should().Be(2);
        }

        [Fact]
        public void With2Buildings_1Above40Percent_1Under40Percent_ThenReturns1Building()
        {
            var buildingAbove40Percent = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingAbove40PercentPersistentLocalId = 1;
            var buildingUnder40Percent = CreateGeometry("200 100 200 200 300 200 300 100 200 100");
            var parcelGeometry = CreateGeometry("139 100 139 200 239 200 239 100 139 100");

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    buildingAbove40PercentPersistentLocalId,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingAbove40Percent.AsBinary(),
                    buildingAbove40Percent,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    2,
                    BuildingGeometryMethod.MeasuredByGrb,
                    buildingUnder40Percent.AsBinary(),
                    buildingUnder40Percent,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext);

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry).ToList();

            result.Count().Should().Be(1);
            result.First().Should().Be(buildingAbove40PercentPersistentLocalId);
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

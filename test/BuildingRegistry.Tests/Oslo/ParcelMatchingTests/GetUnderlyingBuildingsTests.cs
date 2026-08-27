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

        /// <summary>Matching in Lambert 72, the pre-conversion default. See ADR 0006.</summary>
        private static readonly Lambert2008ConversionCompletedToggle Lambert72Matching = new(false);
        private readonly FakeLegacyContext _legacyContext;

        public GetUnderlyingBuildingsTests()
        {
            _legacyContext = new FakeLegacyContextFactory()
                .CreateDbContext([]);
        }

        [Fact]
        public void WithBuildingOverlapping100Percent_ThenReturnsTheUnderlyingBuilding()
        {
            var buildingGeometry = CreateGeometry("140100 186100 140100 186200 140200 186200 140200 186100 140100 186100");
            var parcelGeometry100PercentOverlap = buildingGeometry;

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    WkbWriter.Instance.Write(buildingGeometry),
                    buildingGeometry,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext, Lambert72Matching, new Lambert2008MatchingReadiness());

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry100PercentOverlap);

            result.Should().ContainSingle();
        }

        [Fact]
        public void WithBuildingLessThan80PercentOverlap_ThenReturnsNothing()
        {
            var buildingGeometry = CreateGeometry("140100 186100 140100 186200 140200 186200 140200 186100 140100 186100");
            var parcelGeometry = CreateGeometry("140140 186100 140140 186200 140240 186200 140240 186100 140140 186100");

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    WkbWriter.Instance.Write(buildingGeometry),
                    buildingGeometry,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext, Lambert72Matching, new Lambert2008MatchingReadiness());

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry);

            result.Should().BeEmpty();
        }

        [Fact]
        public void With2BuildingsAbove40PercentOverlap_ThenReturnsThe2Buildings()
        {
            var buildingGeometry50PercentOverlap = CreateGeometry("140050 186100 140050 186200 140140 186200 140140 186100 140050 186100");
            var parcelGeometry = CreateGeometry("140100 186100 140100 186200 140200 186200 140200 186100 140100 186100");

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    1,
                    BuildingGeometryMethod.MeasuredByGrb,
                    WkbWriter.Instance.Write(buildingGeometry50PercentOverlap),
                    buildingGeometry50PercentOverlap,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    2,
                    BuildingGeometryMethod.MeasuredByGrb,
                    WkbWriter.Instance.Write(buildingGeometry50PercentOverlap),
                    buildingGeometry50PercentOverlap,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext, Lambert72Matching, new Lambert2008MatchingReadiness());

            var result = buildingMatching.GetUnderlyingBuildings(parcelGeometry);

            result.Count().Should().Be(2);
        }

        [Fact]
        public void With2Buildings_1Above40Percent_1Under40Percent_ThenReturns1Building()
        {
            var buildingAbove40Percent = CreateGeometry("140100 186100 140100 186200 140200 186200 140200 186100 140100 186100");
            var buildingAbove40PercentPersistentLocalId = 1;
            var buildingUnder40Percent = CreateGeometry("140200 186100 140200 186200 140300 186200 140300 186100 140200 186100");
            var parcelGeometry = CreateGeometry("140139 186100 140139 186200 140239 186200 140239 186100 140139 186100");

            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    buildingAbove40PercentPersistentLocalId,
                    BuildingGeometryMethod.MeasuredByGrb,
                    WkbWriter.Instance.Write(buildingAbove40Percent),
                    buildingAbove40Percent,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.BuildingDetailsV2
                .Add(new BuildingDetailItemV2(
                    2,
                    BuildingGeometryMethod.MeasuredByGrb,
                    WkbWriter.Instance.Write(buildingUnder40Percent),
                    buildingUnder40Percent,
                    BuildingStatus.Realized,
                    false,
                    new Instant()));
            _legacyContext.SaveChanges();

            var buildingMatching = new BuildingMatching(_legacyContext, Lambert72Matching, new Lambert2008MatchingReadiness());

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

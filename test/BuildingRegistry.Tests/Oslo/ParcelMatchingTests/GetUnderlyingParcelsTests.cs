namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System;
    using System.Linq;
    using Api.BackOffice.Abstractions.Building;
    using Api.Oslo.Infrastructure.ParcelMatching;
    using BackOffice;
    using Consumer.Read.Parcel.ParcelWithCount;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class GetUnderlyingParcelsTests
    {
        private readonly FakeConsumerParcelContext _consumerParcelContext;
        private readonly FakeLegacyContext _legacyContext;

        public GetUnderlyingParcelsTests()
        {
            _consumerParcelContext = new FakeConsumerParcelContextFactory()
                .CreateDbContext(Array.Empty<string>());
            _legacyContext = new FakeLegacyContextFactory()
                .CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public void WithParcelOverlapping100Percent_ThenReturnsTheUnderlyingParcel()
        {
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingGeometry100PercentOverlap = parcelGeometry;

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext, _legacyContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry100PercentOverlap.AsBinary());

            result.Should().ContainSingle();
        }

        [Fact]
        public void WithRetiredParcelOverlapping100Percent_ThenReturnsNothing()
        {
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingGeometry100PercentOverlap = parcelGeometry;

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Retired,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext, _legacyContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry100PercentOverlap.AsBinary());

            result.Should().BeEmpty();
        }

        [Fact]
        public void WithParcelLessThan80PercentOverlap_ThenReturnsNothing()
        {
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingGeometry = CreateGeometry("140 100 140 200 240 200 240 100 140 100");

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext, _legacyContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry.AsBinary());

            result.Should().BeEmpty();
        }

        [Fact]
        public void With2ParcelsAbove40PercentOverlap_ThenReturnsThe2Parcels()
        {
            var buildingGeometry50PercentOverlap = CreateGeometry("50 100 50 200 140 200 140 100 50 100");
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext, _legacyContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry50PercentOverlap.AsBinary());

            result.Count().Should().Be(2);
        }

        [Fact]
        public void With2Parcels_1Above40Percent_1Under40Percent_ThenReturns1Parcel()
        {
            var parcelLeft = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var parcelLeftCaPaKey = Guid.NewGuid().ToString();
            var parcelRight = CreateGeometry("200 100 200 200 300 200 300 100 200 100");
            var building = CreateGeometry("139 100 139 200 239 200 239 100 139 100");

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    parcelLeftCaPaKey,
                    ParcelStatus.Realized,
                    parcelLeft.AsBinary(),
                    parcelLeft));

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelRight.AsBinary(),
                    parcelRight));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext, _legacyContext);

            var result = parcelMatching.GetUnderlyingParcels(building.AsBinary()).ToList();

            result.Count.Should().Be(1);
            result.First().Should().Be(parcelLeftCaPaKey);
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

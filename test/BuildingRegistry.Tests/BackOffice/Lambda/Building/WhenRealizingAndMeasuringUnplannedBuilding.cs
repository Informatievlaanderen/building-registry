namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenRealizingAndMeasuringUnplannedBuilding : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public WhenRealizingAndMeasuringUnplannedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenBuildingIsRealizedAndMeasured()
        {
            // Arrange
            var expectedBuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>());

            //Act
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    expectedBuildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest()
                    {
                        BuildingPersistentLocalId = expectedBuildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GeometriePolygoon =
                                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                        }
                    }),
                CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(expectedBuildingPersistentLocalId)), 0, 1);
            var message = stream.Messages.First();
            message.JsonMetadata.Should().Contain(eTagResponse.ETag);
            message.JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildings = Container.Resolve<IBuildings>();

            RealizeAndMeasureUnplannedBuilding(buildingPersistentLocalId);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                buildings);

            //Act
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GeometriePolygoon =
                                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                        }
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, buildingPersistentLocalId),
                            building.LastEventHash)),
                    CancellationToken.None));
        }

        [Fact]
        public async Task GivenRetryingRequest_ThenBuildingIsRealizedAndMeasured()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var buildings = Container.Resolve<IBuildings>();
            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                buildings);

            var request = new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                buildingPersistentLocalId,
                new RealizeAndMeasureUnplannedBuildingSqsRequest()
                {
                    BuildingPersistentLocalId = buildingPersistentLocalId,
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    TicketId = Guid.NewGuid(),
                    Request = new RealizeAndMeasureUnplannedBuildingRequest
                    {
                        GeometriePolygoon =
                            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                        VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                    }
                });

            //Act
            await handler.Handle(request, CancellationToken.None);
            await handler.Handle(request, CancellationToken.None);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, buildingPersistentLocalId),
                            building.LastEventHash)),
                    CancellationToken.None));
        }
    }
}

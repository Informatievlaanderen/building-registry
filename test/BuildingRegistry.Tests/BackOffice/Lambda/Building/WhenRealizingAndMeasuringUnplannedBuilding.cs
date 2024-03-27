namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public partial class WhenRealizingAndMeasuringUnplannedBuilding : BackOfficeLambdaTest
    {
        private const string MainBuildingObjectType = "MainBuilding";

        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly Mock<IPersistentLocalIdGenerator> _persistentLocalIdGenerator;

        public WhenRealizingAndMeasuringUnplannedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
            _persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
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
                Container.Resolve<IBuildings>(),
                Mock.Of<IParcelMatching>(),
                Mock.Of<IAddresses>(),
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container,
                NullLoggerFactory.Instance);

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
                            GrbData = new GrbData
                            {
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =  "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                                GrbObjectType = "0"
                            },

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
                buildings,
                Mock.Of<IParcelMatching>(),
                Mock.Of<IAddresses>(),
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container,
                NullLoggerFactory.Instance);

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
                            GrbData = new GrbData
                            {
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =  "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            }
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
            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                buildings,
                Mock.Of<IParcelMatching>(),
                Mock.Of<IAddresses>(),
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container,
                NullLoggerFactory.Instance);

            var request = new RealizeAndMeasureUnplannedBuildingLambdaRequest(
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
                        GrbData = new GrbData
                        {
                            VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                            GeometriePolygoon =  "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                        }
                    }
                });

            //Act
            await handler.Handle(request, CancellationToken.None);
            await Task.Delay(TimeSpan.FromMilliseconds(200)); // Verify Idempotency timestamp.
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

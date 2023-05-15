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
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
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

    public class WhenMeasuringBuilding : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public WhenMeasuringBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenBuildingIsMeasured()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);
            RealizeBuilding(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());

            var handler = new MeasureBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>());

            //Act
            await handler.Handle(
                new MeasureBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new MeasureBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new MeasureBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                            }
                        }
                    }),
                CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 3, 1);
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

            PlanBuilding(buildingPersistentLocalId);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            var handler = new MeasureBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                buildings);

            //Act
            var request = new MeasureBuildingLambdaRequest(
                buildingPersistentLocalId,
                new MeasureBuildingSqsRequest
                {
                    BuildingPersistentLocalId = buildingPersistentLocalId,
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    TicketId = Guid.NewGuid(),
                    Request = new MeasureBuildingRequest
                    {
                        GrbData = new GrbData
                        {GeometriePolygoon =
                                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                        }
                    }
                });

            await handler.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(string.Empty, "Idempotency"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task GivenRetryingRequest_ThenBuildingIsRealizedAndMeasured()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildings = Container.Resolve<IBuildings>();

            PlanBuilding(buildingPersistentLocalId);

            var handler = new MeasureBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                buildings);

            var request = new MeasureBuildingLambdaRequest(
                buildingPersistentLocalId,
                new MeasureBuildingSqsRequest
                {
                    BuildingPersistentLocalId = buildingPersistentLocalId,
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    TicketId = Guid.NewGuid(),
                    Request = new MeasureBuildingRequest
                    {
                        GrbData = new GrbData
                        {
                            GeometriePolygoon =
                                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                        }
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

        [Fact]
        public async Task WithInvalidPolygon_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new MeasureBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<PolygonIsInvalidException>().Object,
                Container.Resolve<IBuildings>());

            //Act
            await handler.Handle(
                new MeasureBuildingLambdaRequest(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    new MeasureBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>(),
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new MeasureBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                            }
                        }
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig formaat geometriePolygoon.",
                        "GebouwPolygoonValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithBuildingHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new MeasureBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>());

            //Act
            await handler.Handle(
                new MeasureBuildingLambdaRequest(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    new MeasureBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>(),
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new MeasureBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString()
                            }
                        }
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met status 'gepland', 'inAanbouw' of 'gerealiseerd'.",
                        "GebouwGehistoreerdOfNietGerealiseerd"),
                    CancellationToken.None));
        }
    }
}

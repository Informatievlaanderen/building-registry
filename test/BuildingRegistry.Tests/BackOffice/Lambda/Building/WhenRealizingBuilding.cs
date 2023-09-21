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
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
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
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenRealizingBuilding : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public WhenRealizingBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenBuildingIsRealized()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new RealizeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                Mock.Of<ISqsQueue>());

            //Act
            await handler.Handle(CreateRealizeBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task ThenNotificationShouldBeSend()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var sqsQueue = new Mock<ISqsQueue>();
            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);

            var handler = new RealizeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                sqsQueue.Object);

            //Act
            await handler.Handle(CreateRealizeBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            sqsQueue.Verify(x => x.Copy(
                It.IsAny<NotifyOutlinedRealizedBuildingSqsRequest>(),
                It.IsAny<SqsQueueOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task WhenAlreadyRealized_ThenNotificationShouldNotBeSend()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var sqsQueue = new Mock<ISqsQueue>();
            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);
            RealizeBuilding(buildingPersistentLocalId);

            var handler = new RealizeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                sqsQueue.Object);

            //Act
            await handler.Handle(CreateRealizeBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            sqsQueue.Verify(x => x.Copy(
                    It.IsAny<NotifyOutlinedRealizedBuildingSqsRequest>(),
                    It.IsAny<SqsQueueOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task WithIdempotentRequest_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);

            var buildings = Container.Resolve<IBuildings>();
            var handler = new RealizeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>(),
                Mock.Of<ISqsQueue>());

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            // Act
            await handler.Handle(CreateRealizeBuildingLambdaRequest(), CancellationToken.None);

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
        public async Task WhenBuildingHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new RealizeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>(),
                Mock.Of<ISqsQueue>());

            // Act
            await handler.Handle(CreateRealizeBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met status 'inAanbouw'.",
                        "GebouwGehistoreerdGeplandOfNietGerealiseerd"),
                    CancellationToken.None));
        }

        private RealizeBuildingLambdaRequest CreateRealizeBuildingLambdaRequest()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            return new RealizeBuildingLambdaRequest(buildingPersistentLocalId,
                new RealizeBuildingSqsRequest()
                {
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    Request = new RealizeBuildingRequest { PersistentLocalId = buildingPersistentLocalId },
                    TicketId = Guid.NewGuid()
                });
        }
    }
}

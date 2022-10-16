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
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
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

    public class WhenCorrectingBuildingRealization : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public WhenCorrectingBuildingRealization(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenBuildingIsCorrectedToUnderConstruction()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);
            RealizeBuilding(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new CorrectBuildingRealizationLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>());

            //Act
            await handler.Handle(new CorrectBuildingRealizationLambdaRequest
            {
                Request = new BackOfficeCorrectBuildingRealizationRequest { PersistentLocalId = buildingPersistentLocalId },
                MessageGroupId = buildingPersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);

            var buildings = Container.Resolve<IBuildings>();
            var handler = new CorrectBuildingRealizationLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>());

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            // Act
            await handler.Handle(new CorrectBuildingRealizationLambdaRequest
            {
                Request = new BackOfficeCorrectBuildingRealizationRequest { PersistentLocalId = buildingPersistentLocalId },
                MessageGroupId = buildingPersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

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
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var handler = new CorrectBuildingRealizationLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>());

            // Act
            await handler.Handle(new CorrectBuildingRealizationLambdaRequest
            {
                Request = new BackOfficeCorrectBuildingRealizationRequest { PersistentLocalId = buildingPersistentLocalId },
                MessageGroupId = buildingPersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.",
                        "GebouwGeplandOfGehistoreerdOfNietgerealiseerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingGeometryMethodIsMeasuredByGrb_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var handler = new CorrectBuildingRealizationLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidBuildingGeometryMethodException>().Object,
                Container.Resolve<IBuildings>());

            // Act
            await handler.Handle(new CorrectBuildingRealizationLambdaRequest
            {
                Request = new BackOfficeCorrectBuildingRealizationRequest { PersistentLocalId = buildingPersistentLocalId },
                MessageGroupId = buildingPersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met geometriemethode 'ingeschetst'.",
                        "GebouwGeometrieIngemeten"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingHasRetiredBuildingUnits_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var handler = new CorrectBuildingRealizationLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasRetiredBuildingUnitsException>().Object,
                Container.Resolve<IBuildings>());

            // Act
            await handler.Handle(new CorrectBuildingRealizationLambdaRequest
            {
                Request = new BackOfficeCorrectBuildingRealizationRequest { PersistentLocalId = buildingPersistentLocalId },
                MessageGroupId = buildingPersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is niet toegestaan wanneer er gehistoreerde gebouweenheden aanwezig zijn.",
                        "GebouwBevatGehistoreerdeGebouweenheden"),
                    CancellationToken.None));
        }
    }
}

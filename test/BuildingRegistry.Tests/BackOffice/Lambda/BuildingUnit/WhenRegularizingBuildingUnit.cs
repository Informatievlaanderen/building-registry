namespace BuildingRegistry.Tests.BackOffice.Lambda.BuildingUnit
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
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
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

    public class WhenRegularizingBuildingUnit : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public WhenRegularizingBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenBuildingUnitIsRegularized()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);

            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId, hasDeviation: true);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new RegularizeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>());

            //Act
            await handler.Handle(new RegularizeBuildingUnitLambdaRequest(
                    buildingPersistentLocalId,
                    Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new RegularizeBuildingUnitRequest { BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId }),
                CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var buildings = Container.Resolve<IBuildings>();
            var handler = new RegularizeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>());

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            // Act
            await handler.Handle(new RegularizeBuildingUnitLambdaRequest(
                    buildingPersistentLocalId,
                    Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new RegularizeBuildingUnitRequest { BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, buildingUnitPersistentLocalId),
                            building.LastEventHash)),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingUnitHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new RegularizeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>());

            // Act
            await handler.Handle(new RegularizeBuildingUnitLambdaRequest(
                    buildingPersistentLocalId,
                    Guid.NewGuid(),
                    null,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new RegularizeBuildingUnitRequest { BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'.",
                        "GebouweenheidNietGerealiseerdOfGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingUnitHasInvalidFunction_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new RegularizeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitHasInvalidFunctionException>().Object,
                Container.Resolve<IBuildings>());

            // Act
            await handler.Handle(new RegularizeBuildingUnitLambdaRequest(
                buildingPersistentLocalId,
                Guid.NewGuid(),
                string.Empty,
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new RegularizeBuildingUnitRequest { BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId }
            ), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel.",
                        "GebouweenheidGemeenschappelijkDeel"),
                    CancellationToken.None));
        }
    }
}

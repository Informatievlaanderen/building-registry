namespace BuildingRegistry.Tests.BackOffice.Lambda.BuildingUnit
{
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenPlanningBuildingUnit : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;

        public WhenPlanningBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = Container.Resolve<BackOfficeContext>();
        }

        [Fact]
        public async Task ThenBuildingUnitIsPlanned()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var expectedBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            //Act
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(expectedBuildingUnitPersistentLocalId), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var buildingUnitBuilding = _backOfficeContext
                .BuildingUnitBuildings
                .SingleOrDefault(
                    x => x.BuildingPersistentLocalId == buildingPersistentLocalId
                         && x.BuildingUnitPersistentLocalId == expectedBuildingUnitPersistentLocalId);

            buildingUnitBuilding.Should().NotBeNull();
        }

        [Fact]
        public async Task WithTwoPlannedBuildingUnits_ThenCommonBuildingUnitIsAdded()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var expectedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(3);
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedBuildingUnitPersistentLocalId);

            DispatchArrangeCommand(new MigrateBuilding(
                Fixture.Create<BuildingRegistry.Legacy.BuildingId>(),
                new BuildingRegistry.Legacy.PersistentLocalId(buildingPersistentLocalId),
                Fixture.Create<BuildingRegistry.Legacy.PersistentLocalIdAssignmentDate>(),
                BuildingRegistry.Legacy.BuildingStatus.Planned,
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                false,
                new List<BuildingRegistry.Building.Commands.BuildingUnit>
                {
                    new BuildingRegistry.Building.Commands.BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        new BuildingRegistry.Legacy.PersistentLocalId(2),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false)
                },
                Fixture.Create<Provenance>()));

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            //Act
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(expectedBuildingUnitPersistentLocalId), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var buildingUnitBuilding = _backOfficeContext
                .BuildingUnitBuildings
                .SingleOrDefault(
                    x => x.BuildingPersistentLocalId == buildingPersistentLocalId
                         && x.BuildingUnitPersistentLocalId == expectedBuildingUnitPersistentLocalId);
            var commonBuildingUnitBuilding = _backOfficeContext
                .BuildingUnitBuildings
                .SingleOrDefault(
                    x => x.BuildingPersistentLocalId == buildingPersistentLocalId
                         && x.BuildingUnitPersistentLocalId != expectedBuildingUnitPersistentLocalId);

            buildingUnitBuilding.Should().NotBeNull();
            commonBuildingUnitBuilding.Should().NotBeNull();
        }

        [Fact]
        public async Task WhenBuildingHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var expectedBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedBuildingUnitPersistentLocalId);

            PlanBuilding(buildingPersistentLocalId);

            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(expectedBuildingUnitPersistentLocalId), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "De gebouwId is niet gerealiseerd of gehistoreerd.",
                        "GebouweenheidGebouwIdNietGerealiseerdofGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingUnitPositionIsOutsideBuildingGeometry_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var expectedBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedBuildingUnitPersistentLocalId);

            PlanBuilding(buildingPersistentLocalId);

            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitPositionIsOutsideBuildingGeometryException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(expectedBuildingUnitPersistentLocalId), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "De positie dient binnen de geometrie van het gebouw te liggen.",
                        "GebouweenheidOngeldigePositieValidatie"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId,buildingUnitPersistentLocalId);

            var buildings = Container.Resolve<IBuildings>();
            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            //Act
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(buildingUnitPersistentLocalId), CancellationToken.None);

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
        public async Task GivenRetryingRequest_ThenBuildingIsPlanned()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);

            var buildings = Container.Resolve<IBuildings>();
            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                buildings,
                _backOfficeContext);

            //Act
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(buildingUnitPersistentLocalId), CancellationToken.None);
            await handler.Handle(CreatePlanBuildingUnitLambdaRequest(buildingUnitPersistentLocalId), CancellationToken.None);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);
            
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

        private PlanBuildingUnitLambdaRequest CreatePlanBuildingUnitLambdaRequest(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            return new PlanBuildingUnitLambdaRequest(
                buildingPersistentLocalId,
                new PlanBuildingUnitSqsRequest
                {
                    BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    Request = new PlanBuildingUnitRequest
                    {
                        GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                        PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                        Functie = GebouweenheidFunctie.NietGekend,
                        AfwijkingVastgesteld = false
                    },
                    TicketId = Guid.NewGuid()
                });
        }
    }
}

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
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
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
            var persistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            persistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedBuildingUnitPersistentLocalId);

            PlanBuilding(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new PlanBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                persistentLocalIdGenerator.Object);

            //Act
            await handler.Handle(new PlanBuildingUnitLambdaRequest(
                buildingPersistentLocalId,
                Guid.NewGuid(),
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new PlanBuildingUnitRequest
                {
                    GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    Functie = GebouweenheidFunctie.NietGekend,
                    AfwijkingVastgesteld = false
                }), CancellationToken.None);

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
                _backOfficeContext,
                persistentLocalIdGenerator.Object);

            //Act
            await handler.Handle(new PlanBuildingUnitLambdaRequest(
                buildingPersistentLocalId,
                Guid.NewGuid(),
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new PlanBuildingUnitRequest
                {
                    GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    Functie = GebouweenheidFunctie.NietGekend,
                    AfwijkingVastgesteld = false
                }), CancellationToken.None);

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
                _backOfficeContext,
                persistentLocalIdGenerator.Object);

            // Act
            await handler.Handle(new PlanBuildingUnitLambdaRequest(
                buildingPersistentLocalId,
                Guid.NewGuid(),
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new PlanBuildingUnitRequest
                {
                    GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    Functie = GebouweenheidFunctie.NietGekend,
                    AfwijkingVastgesteld = false
                }), CancellationToken.None);

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
                _backOfficeContext,
                persistentLocalIdGenerator.Object);

            // Act
            await handler.Handle(new PlanBuildingUnitLambdaRequest(
                buildingPersistentLocalId,
                Guid.NewGuid(),
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new PlanBuildingUnitRequest
                {
                    GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                    PositieGeometrieMethode = PositieGeometrieMethode.AfgeleidVanObject,
                    Functie = GebouweenheidFunctie.NietGekend,
                    AfwijkingVastgesteld = false
                }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "De positie dient binnen de geometrie van het gebouw te liggen.",
                        "GebouweenheidOngeldigePositieValidatie"),
                    CancellationToken.None));
        }
    }
}

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
    using BuildingRegistry.Building.Datastructures;
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

    public class WhenDemolishingBuilding : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly FakeBackOfficeContext _backOfficeContext;

        public WhenDemolishingBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenBuildingIsDemolished()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            RealizeAndMeasureUnplannedBuilding(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new DemolishBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            //Act
            await handler.Handle(CreateDemolishBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 2, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);
        }

        [Fact]
        public async Task ThenBuildingUnitAddressesAreDetachedInBackOffice()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            RealizeAndMeasureUnplannedBuilding(buildingPersistentLocalId);

            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(101);

            PlanBuildingUnit(buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown);

            var addressPersistentLocalId = new AddressPersistentLocalId(222);

            FakeConsumerAddressContext.AddAddress(addressPersistentLocalId, Consumer.Address.AddressStatus.Current);

            AttachAddressToBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId, addressPersistentLocalId);

            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                addressPersistentLocalId,
                CancellationToken.None);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new DemolishBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            //Act
            await handler.Handle(CreateDemolishBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 6, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var buildingUnitAddressRelation = await _backOfficeContext.FindBuildingUnitAddressRelation(
                buildingUnitPersistentLocalId, addressPersistentLocalId, CancellationToken.None);
            buildingUnitAddressRelation.Should().BeNull();
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
            var handler = new DemolishBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            // Act
            await handler.Handle(CreateDemolishBuildingLambdaRequest(), CancellationToken.None);

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

            var handler = new DemolishBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreateDemolishBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.",
                        "GebouwStatusGeplandInaanbouwNietgerealiseerdGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingHasInvalidGeometryMethod_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new DemolishBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidGeometryMethodException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreateDemolishBuildingLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met geometrieMethode 'ingemetenGRB'.",
                        "GebouwGeometrieMethodeGeschetst"),
                    CancellationToken.None));
        }

        private DemolishBuildingLambdaRequest CreateDemolishBuildingLambdaRequest()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var grbData = Fixture.Create<GrbData>();
            grbData.EndDate = SystemClock.Instance.GetCurrentInstant().ToString();
            grbData.VersionDate = SystemClock.Instance.GetCurrentInstant().ToString();

            return new DemolishBuildingLambdaRequest(buildingPersistentLocalId,
                new DemolishBuildingSqsRequest()
                {
                    BuildingPersistentLocalId = buildingPersistentLocalId,
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    Request = new DemolishBuildingRequest
                    {
                        GrbData = grbData
                    },
                    TicketId = Guid.NewGuid()
                });
        }
    }
}

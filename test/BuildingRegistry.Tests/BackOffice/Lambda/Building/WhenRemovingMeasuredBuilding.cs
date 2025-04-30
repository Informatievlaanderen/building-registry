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
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Consumer.Address;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenRemovingMeasuredBuilding : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly FakeConsumerAddressContext _addressConsumerContext;

        public WhenRemovingMeasuredBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = Container.Resolve<BackOfficeContext>();
            _addressConsumerContext = Container.Resolve<FakeConsumerAddressContext>();
        }

        [Fact]
        public async Task ThenMeasuredBuildingIsRemoved()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var firstBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var firstAddressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var secondBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(firstBuildingUnitPersistentLocalId + 1);
            var secondAddressPersistentLocalId = new AddressPersistentLocalId(firstAddressPersistentLocalId + 1);

            PlanBuilding(buildingPersistentLocalId);
            MeasureBuilding(buildingPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new RemoveMeasuredBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            //Act
            await handler.Handle(
                new RemoveMeasuredBuildingLambdaRequest(
                    buildingPersistentLocalId.ToString(),
                    new RemoveMeasuredBuildingSqsRequest()
                    {
                        TicketId = Guid.NewGuid(),
                        IfMatchHeaderValue = null,
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        Metadata = new Dictionary<string, object?>(),
                        Request = new RemoveMeasuredBuildingRequest
                        {
                            PersistentLocalId = buildingPersistentLocalId
                        }
                    }),
                CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 4, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var firstBuildingUnitAddressRelation = await _backOfficeContext.FindBuildingUnitAddressRelation(
                firstBuildingUnitPersistentLocalId, firstAddressPersistentLocalId, CancellationToken.None);
            firstBuildingUnitAddressRelation.Should().BeNull();

            var secondBuildingUnitAddressRelation = await _backOfficeContext.FindBuildingUnitAddressRelation(
                secondBuildingUnitPersistentLocalId, secondAddressPersistentLocalId, CancellationToken.None);
            secondBuildingUnitAddressRelation.Should().BeNull();
        }

        [Fact]
        public async Task WithIdempotentRequest_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            MeasureBuilding(buildingPersistentLocalId);

            var buildings = Container.Resolve<IBuildings>();
            var handler = new RemoveMeasuredBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            // Act
            await handler.Handle(
                new RemoveMeasuredBuildingLambdaRequest(
                    buildingPersistentLocalId.ToString(),
                    new RemoveMeasuredBuildingSqsRequest
                    {
                        TicketId = Guid.NewGuid(),
                        IfMatchHeaderValue = null,
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        Metadata = new Dictionary<string, object?>(),
                        Request = new RemoveMeasuredBuildingRequest
                        {
                            PersistentLocalId = buildingPersistentLocalId
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
        public async Task WithInvalidBuildingGeometryMethod_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var handler = new RemoveMeasuredBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidGeometryMethodException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new RemoveMeasuredBuildingLambdaRequest(
                    buildingPersistentLocalId.ToString(),
                    new RemoveMeasuredBuildingSqsRequest
                    {
                        TicketId = Guid.NewGuid(),
                        IfMatchHeaderValue = null,
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        Metadata = new Dictionary<string, object?>(),
                        Request = new RemoveMeasuredBuildingRequest
                        {
                            PersistentLocalId = buildingPersistentLocalId
                        }
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met geometrieMethode 'ingemeten'.",
                        "GebouwGeometrieMethodeIngeschetst"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithBuildingUnits_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var handler = new RemoveMeasuredBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasBuildingUnitsException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(
                new RemoveMeasuredBuildingLambdaRequest(
                    buildingPersistentLocalId.ToString(),
                    new RemoveMeasuredBuildingSqsRequest
                    {
                        TicketId = Guid.NewGuid(),
                        IfMatchHeaderValue = null,
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        Metadata = new Dictionary<string, object?>(),
                        Request = new RemoveMeasuredBuildingRequest
                        {
                            PersistentLocalId = buildingPersistentLocalId
                        }
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen zonder gebouweenheden.",
                        "GebouwHeeftGebouweenheden"),
                    CancellationToken.None));
        }
    }
}

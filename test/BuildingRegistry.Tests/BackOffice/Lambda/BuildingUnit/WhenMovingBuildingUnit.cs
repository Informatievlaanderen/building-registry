namespace BuildingRegistry.Tests.BackOffice.Lambda.BuildingUnit
{
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenMovingBuildingUnit : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly FakeBackOfficeContext _backOfficeContext;

        public WhenMovingBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext([]);
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext([]);
        }

        [Fact]
        public async Task ThenBuildingUnitIsMoved()
        {
            // Arrange
            var sourceBuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var destinationBuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(sourceBuildingPersistentLocalId);
            PlanBuildingUnit(sourceBuildingPersistentLocalId, buildingUnitPersistentLocalId);
            PlanBuilding(destinationBuildingPersistentLocalId);
            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(sourceBuildingPersistentLocalId, buildingUnitPersistentLocalId, CancellationToken.None);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(sourceBuildingPersistentLocalId, buildingUnitPersistentLocalId, Fixture.Create<AddressPersistentLocalId>(), CancellationToken.None);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new MoveBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);
            
            //Act
            await handler.Handle(CreateMoveBuildingUnitLambdaRequest(sourceBuildingPersistentLocalId, destinationBuildingPersistentLocalId),
                CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(destinationBuildingPersistentLocalId)), 1, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            //TODO-rik add 2nd command testing

            var buildingRelation = await _backOfficeContext.FindBuildingUnitBuildingRelation(buildingUnitPersistentLocalId, CancellationToken.None);
            buildingRelation.Should().NotBeNull();
            buildingRelation!.BuildingPersistentLocalId.Should().Be(destinationBuildingPersistentLocalId);

            var destinationAddressRelations = await _backOfficeContext.FindAllBuildingUnitAddressRelations(buildingUnitPersistentLocalId, CancellationToken.None);
            destinationAddressRelations.Should().ContainSingle();
            destinationAddressRelations.Single().BuildingPersistentLocalId.Should().Be(destinationBuildingPersistentLocalId);
        }

        [Fact]
        public async Task WithIdempotentRequest_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var sourceBuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var destinationBuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(destinationBuildingPersistentLocalId);
            PlanBuildingUnit(destinationBuildingPersistentLocalId, buildingUnitPersistentLocalId);
            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(sourceBuildingPersistentLocalId, buildingUnitPersistentLocalId, CancellationToken.None);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(sourceBuildingPersistentLocalId, buildingUnitPersistentLocalId, Fixture.Create<AddressPersistentLocalId>(), CancellationToken.None);

            var buildings = Container.Resolve<IBuildings>();
            var handler = new MoveBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var destinationBuilding =
                await buildings.GetAsync(new BuildingStreamId(destinationBuildingPersistentLocalId), CancellationToken.None);

            //Act
            await handler.Handle(CreateMoveBuildingUnitLambdaRequest(sourceBuildingPersistentLocalId, destinationBuildingPersistentLocalId),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    It.IsAny<Guid>(),
                    new TicketResult(
                        new ETagResponse(
                            string.Format(ConfigDetailUrl, buildingUnitPersistentLocalId),
                            destinationBuilding.LastEventHash)),
                    CancellationToken.None));
            //TODO-rik add 2nd command testing

            var buildingRelation = await _backOfficeContext.FindBuildingUnitBuildingRelation(buildingUnitPersistentLocalId, CancellationToken.None);
            buildingRelation.Should().NotBeNull();
            buildingRelation!.BuildingPersistentLocalId.Should().Be(destinationBuildingPersistentLocalId);

            var destinationAddressRelations = await _backOfficeContext.FindAllBuildingUnitAddressRelations(buildingUnitPersistentLocalId, CancellationToken.None);
            destinationAddressRelations.Should().ContainSingle();
            destinationAddressRelations.Single().BuildingPersistentLocalId.Should().Be(destinationBuildingPersistentLocalId);
        }

        [Fact]
        public async Task WithInvalidBuildingUnitStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new MoveBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreateMoveBuildingUnitLambdaRequest(), CancellationToken.None);

            //Assert
            //TODO-rik confirm strings
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'.",
                        "TODO-rik"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidBuildingStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new MoveBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreateMoveBuildingUnitLambdaRequest(), CancellationToken.None);

            //Assert
            //TODO-rik confirm strings
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw.",
                        "TODO-rik"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidBuildingUnitFunction_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new MoveBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitHasInvalidFunctionException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            // Act
            await handler.Handle(CreateMoveBuildingUnitLambdaRequest(), CancellationToken.None);

            //Assert
            //TODO-rik confirm strings
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel.",
                        "GebouweenheidGemeenschappelijkDeel"),
                    CancellationToken.None));
        }

        private MoveBuildingUnitLambdaRequest CreateMoveBuildingUnitLambdaRequest(
            BuildingPersistentLocalId? sourceBuildingPersistentLocalId = null,
            BuildingPersistentLocalId? destinationBuildingPersistentLocalId = null)
        {
            return new MoveBuildingUnitLambdaRequest(sourceBuildingPersistentLocalId ?? Fixture.Create<BuildingPersistentLocalId>(),
                new MoveBuildingUnitSqsRequest
                {
                    BuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>(),
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    Request = new MoveBuildingUnitRequest
                    {
                        DoelgebouwId = PuriCreator.CreateBuildingId(destinationBuildingPersistentLocalId ?? Fixture.Create<BuildingPersistentLocalId>())
                    },
                    TicketId = Guid.NewGuid()
                });
        }
    }
}

namespace BuildingRegistry.Tests.BackOffice.Lambda.BuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Builders;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit;
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

    public class WhenDetachingAddressFromBuildingUnit : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly FakeBackOfficeContext _backOfficeContext;

        public WhenDetachingAddressFromBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenAddressIsDetachedFromBuildingUnit()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);
            RealizeBuilding(buildingPersistentLocalId);

            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new DetachAddressFromBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var addressId = Fixture.Create<AddressPersistentLocalId>();
            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            AttachBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId, addressId);

            var request = new DetachAddressFromBuildingUnitLambdaRequestBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithAdresId(addressId)
                .Build();

            //Act
            await handler.Handle(request, CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 5, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var relation = _backOfficeContext.BuildingUnitAddressRelation.Find((int)buildingUnitPersistentLocalId, (int) addressId);
            relation.Should().BeNull();
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
            var handler = new DetachAddressFromBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var building =
                await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), CancellationToken.None);

            var addressId = Fixture.Create<AddressPersistentLocalId>();
            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            var request = new DetachAddressFromBuildingUnitLambdaRequestBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithAdresId(addressId)
                .Build();

            // Act
            await handler.Handle(request, CancellationToken.None);

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
        public async Task WhenBuildingUnitAddressRelationDoesNotExists_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlaceBuildingUnderConstruction(buildingPersistentLocalId);
            RealizeBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());
            var handler = new DetachAddressFromBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            AttachBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId, addressPersistentLocalId);

            var request = new DetachAddressFromBuildingUnitLambdaRequestBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithAdresId(addressPersistentLocalId)
                .Build();

            //Act
            await handler.Handle(request, CancellationToken.None);

            //Assert
            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 5, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            var buildingUnitAddressRelation = _backOfficeContext.BuildingUnitAddressRelation.SingleOrDefault(
                x => x.BuildingPersistentLocalId == buildingPersistentLocalId
                     && x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId
                     && x.AddressPersistentLocalId == addressPersistentLocalId);
            buildingUnitAddressRelation.Should().BeNull();
        }


        [Fact]
        public async Task WhenAddressNotFound_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new DetachAddressFromBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressNotFoundException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var addressId = Fixture.Create<AddressPersistentLocalId>();
            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            var request = new DetachAddressFromBuildingUnitLambdaRequestBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithAdresId(addressId)
                .Build();

            // Act
            await handler.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig AdresId.",
                        "AdresOngeldig"),
                    CancellationToken.None));
        }


        [Fact]
        public async Task WhenAddressIsRemoved_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var handler = new DetachAddressFromBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressIsRemovedException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext);

            var addressId = Fixture.Create<AddressPersistentLocalId>();
            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(addressId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            var request = new DetachAddressFromBuildingUnitLambdaRequestBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithAdresId(addressId)
                .Build();

            // Act
            await handler.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig AdresId.",
                        "AdresOngeldig"),
                    CancellationToken.None));
        }

    }
}

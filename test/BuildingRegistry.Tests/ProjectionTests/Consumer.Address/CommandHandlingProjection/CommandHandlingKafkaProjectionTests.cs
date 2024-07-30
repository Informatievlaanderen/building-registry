namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address.CommandHandlingProjection
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using BuildingRegistry.Consumer.Address;
    using BuildingRegistry.Consumer.Address.Projections;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using Tests.BackOffice;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Xunit.Abstractions;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public partial class CommandHandlingKafkaProjectionTests : KafkaProjectionTest<CommandHandler, CommandHandlingKafkaProjection>
    {
        private readonly FakeBackOfficeContext _fakeBackOfficeContext;
        private readonly Mock<FakeCommandHandler> _mockCommandHandler;
        private readonly Mock<IBuildings> _buildings;

        public CommandHandlingKafkaProjectionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());

            _mockCommandHandler = new Mock<FakeCommandHandler>();
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory(true).CreateDbContext([]);
            _buildings = new Mock<IBuildings>();
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseRemovedAddressWasMigrated()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: string.Empty,
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: true,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId,addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseRejectedAddressWasMigrated()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: AddressStatus.Rejected,
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId, addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseRetiredAddressWasMigrated()
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: AddressStatus.Retired,
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId, addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Theory]
        [InlineData("Current")]
        [InlineData("Proposed")]
        public async Task DoNothingWhenAddressStatus(string status)
        {
            var addressPersistentLocalId = 456;

            var @event = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId: 0,
                addressId: string.Empty,
                streetNameId: string.Empty,
                addressPersistentLocalId: addressPersistentLocalId,
                status: AddressStatus.Parse(status),
                houseNumber: string.Empty,
                boxNumber: string.Empty,
                geometryMethod: string.Empty,
                geometrySpecification: string.Empty,
                extendedWkbGeometry: string.Empty,
                officiallyAssigned: true,
                postalCode: string.Empty,
                isCompleted: false,
                isRemoved: false,
                parentPersistentLocalId: null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(addressPersistentLocalId, addressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>(), CancellationToken.None),
                    Times.Never);
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None),
                    Times.Never);
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None),
                    Times.Never);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseAddressWasRemoved()
        {
            var addressIntId = 456;

            var @event = new AddressWasRemovedV2(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var addressIntId = 456;

            var @event = new AddressWasRemovedBecauseHouseNumberWasRemoved(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseAddressWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseHouseNumberWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecause_AddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseHouseNumberWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecause_AddressWasRejectedBecauseStreetNameWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseStreetNameWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger_AddressWasRejectedBecauseOfMunicipalityMerger()
        {
            var oldAddressPersistentLocalId = 1;
            var newAddressPersistentLocalId = 2;

            var @event = new AddressWasRejectedBecauseOfMunicipalityMerger(
                Fixture.Create<int>(),
                oldAddressPersistentLocalId,
                newAddressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(oldAddressPersistentLocalId, oldAddressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(
                    It.IsAny<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>(), CancellationToken.None),
                    Times.Exactly(2));

                var oldAddressRelations = _fakeBackOfficeContext.BuildingUnitAddressRelation
                    .Where(x => x.AddressPersistentLocalId == oldAddressPersistentLocalId)
                    .ToList();

                var newAddressRelations = _fakeBackOfficeContext.BuildingUnitAddressRelation
                    .Where(x => x.AddressPersistentLocalId == newAddressPersistentLocalId)
                    .ToList();

                oldAddressRelations.Should().BeEmpty();
                newAddressRelations.Should().HaveCount(2);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachBuildingUnitAddressBecauseOfMunicipalityMergerWhenNoNewAddress_AddressWasRejectedBecauseOfMunicipalityMerger()
        {
            var oldAddressPersistentLocalId = 1;

            var @event = new AddressWasRejectedBecauseOfMunicipalityMerger(
                Fixture.Create<int>(),
                oldAddressPersistentLocalId,
                null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(oldAddressPersistentLocalId, oldAddressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(
                    It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None),
                    Times.Exactly(2));

                var oldAddressRelations = _fakeBackOfficeContext.BuildingUnitAddressRelation
                    .Where(x => x.AddressPersistentLocalId == oldAddressPersistentLocalId)
                    .ToList();

                oldAddressRelations.Should().BeEmpty();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecause_AddressWasRejectedBecauseStreetNameWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseStreetNameWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseAddressWasRetiredV2()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredV2(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseHouseNumberWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseHouseNumberWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseStreetNameWasRejected()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseStreetNameWasRejected(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseStreetNameWasRetired()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseStreetNameWasRetired(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger_AddressWasRetiredBecauseOfMunicipalityMerger()
        {
            var oldAddressPersistentLocalId = 1;
            var newAddressPersistentLocalId = 2;

            var @event = new AddressWasRetiredBecauseOfMunicipalityMerger(
                Fixture.Create<int>(),
                oldAddressPersistentLocalId,
                newAddressPersistentLocalId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(oldAddressPersistentLocalId, oldAddressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(
                        It.IsAny<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>(), CancellationToken.None),
                    Times.Exactly(2));

                var oldAddressRelations = _fakeBackOfficeContext.BuildingUnitAddressRelation
                    .Where(x => x.AddressPersistentLocalId == oldAddressPersistentLocalId)
                    .ToList();

                var newAddressRelations = _fakeBackOfficeContext.BuildingUnitAddressRelation
                    .Where(x => x.AddressPersistentLocalId == newAddressPersistentLocalId)
                    .ToList();

                oldAddressRelations.Should().BeEmpty();
                newAddressRelations.Should().HaveCount(2);
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachBuildingUnitAddressBecauseOfMunicipalityMergerWhenNoNewAddress_AddressWasRetiredBecauseOfMunicipalityMerger()
        {
            var oldAddressPersistentLocalId = 1;

            var @event = new AddressWasRetiredBecauseOfMunicipalityMerger(
                Fixture.Create<int>(),
                oldAddressPersistentLocalId,
                null,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(oldAddressPersistentLocalId, oldAddressPersistentLocalId);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(
                        It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None),
                    Times.Exactly(2));

                var oldAddressRelations = _fakeBackOfficeContext.BuildingUnitAddressRelation
                    .Where(x => x.AddressPersistentLocalId == oldAddressPersistentLocalId)
                    .ToList();

                oldAddressRelations.Should().BeEmpty();
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnitBecauseStreetNameWasRemoved()
        {
            var addressIntId = 456;

            var @event = new AddressWasRemovedBecauseStreetNameWasRemoved(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnit_BecauseAddressWasRejectedBecauseOfReaddress()
        {
            var addressIntId = 456;

            var @event = new AddressWasRejectedBecauseOfReaddress(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRejected>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task DetachAddressFromBuildingUnit_BecauseAddressWasRetiredBecauseOfReaddress()
        {
            var addressIntId = 456;

            var @event = new AddressWasRetiredBecauseOfReaddress(
                123,
                addressIntId,
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.BuildingRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            AddRelations(456, 456);

            Given(@event);
            await Then(async _ =>
            {
                _mockCommandHandler.Verify(x => x.Handle(It.IsAny<DetachAddressFromBuildingUnitBecauseAddressWasRetired>(), CancellationToken.None), Times.Exactly(2));
                await Task.CompletedTask;
            });
        }

        private void AddRelations(params int[] addressInts)
        {
            foreach (var addressInt in addressInts)
            {
                _fakeBackOfficeContext.BuildingUnitAddressRelation.Add(
                    new BuildingUnitAddressRelation(Fixture.Create<BuildingPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPersistentLocalId>(),
                        new AddressPersistentLocalId(addressInt)));
            }

            _fakeBackOfficeContext.SaveChanges();
        }

        protected override CommandHandler CreateContext()
        {
            return _mockCommandHandler.Object;
        }

        protected override CommandHandlingKafkaProjection CreateProjection()
        {
            var factoryMock = new Mock<IDbContextFactory<BackOfficeContext>>();
            factoryMock
                .Setup(x => x.CreateDbContextAsync(CancellationToken.None))
                .Returns(Task.FromResult<BackOfficeContext>(_fakeBackOfficeContext));
            return new CommandHandlingKafkaProjection(factoryMock.Object, _buildings.Object);
        }
    }

    public class FakeCommandHandler : CommandHandler
    {
        public FakeCommandHandler() : base(null, new NullLoggerFactory())
        { }
    }
}

namespace BuildingRegistry.Tests.AggregateTests.WhenAttachingAddressToBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Consumer.Address;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingWithBuildingUnit : BuildingRegistryTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenApplyBuildingUnitAddressWasAttachedV2()
        {
            var command = Fixture.Create<AttachAddressToBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    isRemoved: false)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.AddressPersistentLocalId, AddressStatus.Current, isRemoved: false);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitAddressWasAttachedV2(command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId, command.AddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedBuilder(Fixture)
                .WithBuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>())
                .WithBuildingUnitPersistentLocalId(Fixture.Create<BuildingUnitPersistentLocalId>())
                .WithAddressPersistentLocalId(Fixture.Create<AddressPersistentLocalId>())
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(new BuildingPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingPersistentLocalId))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                    isRemoved: false)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId), AddressStatus.Current, isRemoved: false);

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasAttachedV2
            });

            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().BeEquivalentTo(new List<AddressPersistentLocalId>{ new(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId) });
        }
    }
}

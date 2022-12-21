namespace BuildingRegistry.Tests.AggregateTests.WhenDetachingRemovedAddressFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnit : BuildingRegistryTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenApplyBuildingUnitAddressWasDetachedBecauseAddressWasRemoved()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddress: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitAddressWasDetachedBecauseAddressWasRemoved(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.AddressPersistentLocalId))));
        }

        [Fact]
        public void WithAddressNoLongerAttached_ThenNothing()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddress: new List<AddressPersistentLocalId>(0),
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitAddressWasDetachedBecauseAddressWasRemoved = Fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>();
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(new BuildingPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.BuildingPersistentLocalId))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.BuildingUnitPersistentLocalId),
                    attachedAddress: new List<AddressPersistentLocalId>
                    {
                        new AddressPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId),
                        new AddressPersistentLocalId(123),
                    },
                    isRemoved: false)
                .Build();

            var building = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();
            building.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasDetachedBecauseAddressWasRemoved
            });

            building.BuildingUnits.First().AddressPersistentLocalIds.Should().BeEquivalentTo(
                new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(123) });
        }
    }
}

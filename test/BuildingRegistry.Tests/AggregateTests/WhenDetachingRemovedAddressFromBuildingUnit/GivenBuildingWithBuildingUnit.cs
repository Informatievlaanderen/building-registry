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
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

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
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
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
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>(0),
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
            var expectedPersistentLocalId = buildingUnitAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId + 1;
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(new BuildingPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.BuildingPersistentLocalId))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.BuildingUnitPersistentLocalId),
                    attachedAddresses: new List<AddressPersistentLocalId>
                    {
                        new AddressPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId),
                        new AddressPersistentLocalId(expectedPersistentLocalId),
                    },
                    isRemoved: false)
                .Build();
            // Below event is used to add the address persistent local id twice.
            var buildingUnitAddressWasReplacedBecauseAddressWasReaddressed = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                new BuildingPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(expectedPersistentLocalId + 1),
                new AddressPersistentLocalId(buildingUnitAddressWasDetachedBecauseAddressWasRemoved.AddressPersistentLocalId));

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasReplacedBecauseAddressWasReaddressed,
                buildingUnitAddressWasDetachedBecauseAddressWasRemoved
            });

            building.BuildingUnits.First().AddressPersistentLocalIds.Should().BeEquivalentTo(
                new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(expectedPersistentLocalId) });
        }

        [Fact]
        public void WithUnusedCommonUnit_ThenApplyBuildingUnitAddressWasDetachedBecauseAddressWasRemoved()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId + 1),
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
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
        public void WithUnusedCommonUnitAndAddressNoLongerAttached_ThenNothing()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId + 1),
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { },
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }
    }
}

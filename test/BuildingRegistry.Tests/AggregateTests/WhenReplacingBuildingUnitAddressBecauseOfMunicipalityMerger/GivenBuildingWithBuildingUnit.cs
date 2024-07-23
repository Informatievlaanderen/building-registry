#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Tests.AggregateTests.WhenReplacingBuildingUnitAddressBecauseOfMunicipalityMerger
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
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingWithBuildingUnit : BuildingRegistryTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger()
        {
            var command = Fixture.Create<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { command.PreviousAddressPersistentLocalId },
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId), buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.NewAddressPersistentLocalId, command.PreviousAddressPersistentLocalId))));
        }

        [Fact]
        public void WithAddressNoLongerAttached_ThenNothing()
        {
            var command = Fixture.Create<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: [],
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId), buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithAddressIsAttachedToUnusedCommonBuildingUnit_ThenBuildingUnitAddressWasDetachedV2()
        {
            var command = Fixture.Create<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId + 1),
                    attachedAddresses: [],
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                    attachedAddresses: [command.PreviousAddressPersistentLocalId],
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitAddressWasDetachedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PreviousAddressPersistentLocalId))));
        }

        [Fact]
        public void WithUnusedCommonBuildingUnitAndAddressNoLongerAttached_ThenNothing()
        {
            var command = Fixture.Create<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId + 1),
                    attachedAddresses: [],
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                    attachedAddresses: [],
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    isRemoved: true)
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
            var oldAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var newAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitAddressWasReplacedBecauseOfMunicipalityMerger = new BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                newAddressPersistentLocalId, oldAddressPersistentLocalId);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>
                    {
                        oldAddressPersistentLocalId,
                        new AddressPersistentLocalId(3),
                    },
                    isRemoved: false)
                .Build();


            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasReplacedBecauseOfMunicipalityMerger
            });

            var buildingUnit = building.BuildingUnits.Single();

            buildingUnit.AddressPersistentLocalIds.Should().HaveCount(2);
            var replacedAddress = buildingUnit.AddressPersistentLocalIds.SingleOrDefault(x => x == newAddressPersistentLocalId);
            replacedAddress.Should().NotBeNull();
        }
    }
}

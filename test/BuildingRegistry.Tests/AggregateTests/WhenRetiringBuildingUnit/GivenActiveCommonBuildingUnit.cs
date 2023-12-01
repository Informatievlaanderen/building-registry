namespace BuildingRegistry.Tests.AggregateTests.WhenRetiringBuildingUnit
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;

    public class GivenActiveCommonBuildingUnit : BuildingRegistryTest
    {
        public GivenActiveCommonBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithRealizedCommonBuildingUnit_ThenCommonBuildingUnitWasRetired()
        {
            var command = Fixture.Create<RetireBuildingUnit>();

            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var attachAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common,
                    attachedAddresses: new List<AddressPersistentLocalId> { attachAddressPersistentLocalId })
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRetiredV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(command.BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId, attachAddressPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRetiredV2(command.BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId))
                ));
        }

        [Fact]
        public void WithPlannedCommonBuildingUnit_ThenCommonBuildingUnitWasNotRealized()
        {
            var command = Fixture.Create<RetireBuildingUnit>();

            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId), new BuildingUnitWasRetiredV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId), new BuildingUnitWasNotRealizedV2(command.BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId))
                ));
        }

        [Fact]
        public void WithPlannedAndRetiredCommonBuildingUnits_ThenPlannedCommonBuildingUnitWasNotRealized()
        {
            var command = Fixture.Create<RetireBuildingUnit>();

            var plannedCommonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    plannedCommonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId), new BuildingUnitWasRetiredV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId), new BuildingUnitWasNotRealizedV2(command.BuildingPersistentLocalId, plannedCommonBuildingUnitPersistentLocalId))
                ));
        }
    }
}

namespace BuildingRegistry.Tests.AggregateTests.WhenMarkingBuildingAsMerged
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public partial class GivenBuildingDoesExist : BuildingRegistryTest
    {
        public GivenBuildingDoesExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenBuildingWasMerged()
        {
            var command = new MarkBuildingAsMerged(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var plannedBuilding = new BuildingWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)plannedBuilding).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
                    plannedBuilding)
                .When(command)
                .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasMerged(command.BuildingPersistentLocalId,
                        command.DestinationBuildingPersistentLocalId)));
        }

        [Fact]
        public void WithOneBuildingUnit_ThenBuildingUnitWasMoved()
        {
            var command = new MarkBuildingAsMerged(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var plannedBuilding = new BuildingWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)plannedBuilding).SetProvenance(Fixture.Create<Provenance>());

            var plannedBuildingUnit = new BuildingUnitWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                false);
            ((ISetProvenance)plannedBuildingUnit).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
                    plannedBuilding,
                    plannedBuildingUnit)
                .When(command)
                .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(plannedBuildingUnit.BuildingUnitPersistentLocalId),
                        command.DestinationBuildingPersistentLocalId),
                    new BuildingWasMerged(command.BuildingPersistentLocalId,
                        command.DestinationBuildingPersistentLocalId)
                ));
        }

        [Fact]
        public void WithThreeBuildingUnit_ThenBuildingUnitWasMoved()
        {
            var command = new MarkBuildingAsMerged(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var plannedBuilding = new BuildingWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)plannedBuilding).SetProvenance(Fixture.Create<Provenance>());

            var plannedBuildingUnit1 = new BuildingUnitWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                false);
            ((ISetProvenance)plannedBuildingUnit1).SetProvenance(Fixture.Create<Provenance>());

            var plannedBuildingUnit2 = new BuildingUnitWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                false);
            ((ISetProvenance)plannedBuildingUnit2).SetProvenance(Fixture.Create<Provenance>());

            var commonBuildingUnit = new BuildingUnitWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Common,
                false);
            ((ISetProvenance)commonBuildingUnit).SetProvenance(Fixture.Create<Provenance>());

            var plannedBuildingUnit3 = new BuildingUnitWasPlannedV2(
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                false);
            ((ISetProvenance)plannedBuildingUnit3).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedV2(command.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(commonBuildingUnit.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(1));
            ((ISetProvenance)buildingUnitAddressWasAttached).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
                    plannedBuilding,
                    plannedBuildingUnit1,
                    plannedBuildingUnit2,
                    commonBuildingUnit,
                    buildingUnitAddressWasAttached,
                    plannedBuildingUnit3)
                .When(command)
                .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(plannedBuildingUnit1.BuildingUnitPersistentLocalId),
                        command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(plannedBuildingUnit2.BuildingUnitPersistentLocalId),
                        command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitAddressWasDetachedV2(command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(commonBuildingUnit.BuildingUnitPersistentLocalId),
                        new AddressPersistentLocalId(1)),
                    new BuildingUnitWasNotRealizedV2(command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(commonBuildingUnit.BuildingUnitPersistentLocalId)),
                    new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(plannedBuildingUnit3.BuildingUnitPersistentLocalId),
                        command.DestinationBuildingPersistentLocalId),
                    new BuildingWasMerged(command.BuildingPersistentLocalId,
                        command.DestinationBuildingPersistentLocalId)
                ));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidBuildingUnitStatus_ThenBuildingUnitWasNotMoved(string buildingUnitStatus)
        {
            var command = new MarkBuildingAsMerged(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(BuildingUnitStatus.Parse(buildingUnitStatus).Value)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasMerged(command.BuildingPersistentLocalId,
                        command.DestinationBuildingPersistentLocalId)
                ));
        }

        [Fact]
        public void WithRemovedBuildingUnitStatus_ThenBuildingUnitWasNotMoved()
        {
            var command = new MarkBuildingAsMerged(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(BuildingUnitStatus.Planned, isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasMerged(command.BuildingPersistentLocalId,
                        command.DestinationBuildingPersistentLocalId)
                ));
        }

        // TODO:
        // building units in invalid statuses (notRealized, retired, removed)
        // state check (remove bu from list)
    }
}

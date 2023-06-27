namespace BuildingRegistry.Tests.AggregateTests.WhenMarkingBuildingAsMerged
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public partial class GivenBuildingDoesExist : BuildingRegistryTest
    {
        [Fact]
        public void StateCheck()
        {
            var buildingWasPlanned = new BuildingWasPlannedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>());
            buildingWasPlanned.SetFixtureProvenance(Fixture);

            var buildingUnitWasPlanned = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Unknown,
                false);
            buildingUnitWasPlanned.SetFixtureProvenance(Fixture);

            var commonBuildingUnitWasPlanned = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                BuildingUnitFunction.Common,
                false);
            commonBuildingUnitWasPlanned.SetFixtureProvenance(Fixture);

            var commonBuildingUnitWasNotRealized = new BuildingUnitWasNotRealizedV2(
                new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(commonBuildingUnitWasPlanned.BuildingUnitPersistentLocalId));
            commonBuildingUnitWasNotRealized.SetFixtureProvenance(Fixture);

            var buildingWasMerged = new BuildingWasMerged(
                new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
                Fixture.Create<BuildingPersistentLocalId>());
            buildingWasMerged.SetFixtureProvenance(Fixture);

            var buildingUnitWasMoved = new BuildingUnitWasMoved(
                new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId),
                new BuildingPersistentLocalId(buildingWasMerged.DestinationBuildingPersistentLocalId));
            buildingUnitWasMoved.SetFixtureProvenance(Fixture);


            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize(new object[]
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                commonBuildingUnitWasPlanned,
                commonBuildingUnitWasNotRealized,
                buildingUnitWasMoved,
                buildingWasMerged
            });

            building.BuildingStatus.Should().Be(BuildingStatus.Retired);
            building.BuildingUnits
                .FirstOrDefault(x =>
                    x.BuildingUnitPersistentLocalId ==
                    new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId))
                .Should()
                .BeNull();
            var commonBuildingUnit = building.BuildingUnits
                .FirstOrDefault(x =>
                    x.BuildingUnitPersistentLocalId ==
                    new BuildingUnitPersistentLocalId(commonBuildingUnitWasPlanned.BuildingUnitPersistentLocalId));
            commonBuildingUnit.Should().NotBeNull();
            commonBuildingUnit.Status.Should().Be(BuildingUnitStatus.NotRealized);
        }
    }
}

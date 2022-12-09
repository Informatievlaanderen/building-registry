namespace BuildingRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Building;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class TakeBuildingSnapshotTests : BuildingRegistryTest
    {
        public TakeBuildingSnapshotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public void BuildingWasPlannedIsSavedInSnapshot()
        {
            var aggregate = new BuildingFactory(IntervalStrategy.Default, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            aggregate.Initialize(new List<object> { buildingWasPlanned });

            var snapshot = aggregate.TakeSnapshot();
            snapshot.Should().BeOfType<BuildingSnapshot>();

            var buildingSnapshot = (BuildingSnapshot)snapshot;
            buildingSnapshot.BuildingPersistentLocalId.Should().Be(buildingWasPlanned.BuildingPersistentLocalId);
            buildingSnapshot.IsRemoved.Should().BeFalse();
            buildingSnapshot.BuildingStatus.Should().Be(BuildingStatus.Parse(buildingSnapshot.BuildingStatus));
            buildingSnapshot.ExtendedWkbGeometry.Should().Be(buildingWasPlanned.ExtendedWkbGeometry);
            buildingSnapshot.GeometryMethod.Should().Be(BuildingGeometryMethod.Outlined.Value);
            buildingSnapshot.LastEventHash.Should().Be(buildingWasPlanned.GetHash());
            buildingSnapshot.LastProvenanceData.Should().Be(buildingWasPlanned.Provenance);
        }

        [Fact]
        public void BuildingWasMigratedIsSavedInSnapshot()
        {
            var aggregate = new BuildingFactory(IntervalStrategy.Default, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();

            var buildingWasMigrated = Fixture.Create<BuildingWasMigrated>();

            aggregate.Initialize(new List<object> { buildingWasMigrated });

            var snapshot = aggregate.TakeSnapshot();
            snapshot.Should().BeOfType<BuildingSnapshot>();

            var buildingSnapshot = (BuildingSnapshot)snapshot;
            buildingSnapshot.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
            buildingSnapshot.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
            buildingSnapshot.BuildingStatus.Should().Be(BuildingStatus.Parse(buildingSnapshot.BuildingStatus));
            buildingSnapshot.ExtendedWkbGeometry.Should().Be(buildingWasMigrated.ExtendedWkbGeometry);
            buildingSnapshot.GeometryMethod.Should().Be(BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod));
            buildingSnapshot.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
            buildingSnapshot.LastProvenanceData.Should().Be(buildingWasMigrated.Provenance);

            foreach (var buildingUnitData in buildingSnapshot.BuildingUnits)
            {
                var expectedBuildingUnit = buildingWasMigrated.BuildingUnits.Single(x =>
                    x.BuildingUnitPersistentLocalId == buildingUnitData.BuildingUnitPersistentLocalId);

                buildingUnitData.Function.Should().Be(expectedBuildingUnit.Function);
                buildingUnitData.Status.Should().Be(expectedBuildingUnit.Status);
                buildingUnitData.AddressPersistentLocalIds.Should().BeEquivalentTo(expectedBuildingUnit.AddressPersistentLocalIds);
                buildingUnitData.ExtendedWkbGeometry.Should().Be(expectedBuildingUnit.ExtendedWkbGeometry);
                buildingUnitData.GeometryMethod.Should().Be(expectedBuildingUnit.GeometryMethod);

                buildingUnitData.IsRemoved.Should().Be(expectedBuildingUnit.IsRemoved);
                buildingUnitData.HasDeviation.Should().BeFalse();

                buildingUnitData.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
                buildingUnitData.LastProvenanceData.Should().Be(buildingWasMigrated.Provenance);
            }
        }

        [Fact]
        public void BuildingUnitWasPlannedIsSavedInSnapshot()
        {
            var aggregate = new BuildingFactory(IntervalStrategy.Default, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();

            aggregate.Initialize(new List<object>
            {
                buildingWasPlanned,
                buildingUnitWasPlanned
            });

            var snapshot = aggregate.TakeSnapshot();
            snapshot.Should().BeOfType<BuildingSnapshot>();

            var buildingSnapshot = (BuildingSnapshot)snapshot;
            var buildingUnitData = buildingSnapshot.BuildingUnits.Single(x =>
                x.BuildingUnitPersistentLocalId == buildingUnitWasPlanned.BuildingUnitPersistentLocalId);

            buildingUnitData.Function.Should().Be(buildingUnitWasPlanned.Function);
            buildingUnitData.Status.Should().Be(BuildingUnitStatus.Planned);
            buildingUnitData.AddressPersistentLocalIds.Should().BeNullOrEmpty();
            buildingUnitData.ExtendedWkbGeometry.Should().Be(buildingUnitWasPlanned.ExtendedWkbGeometry);
            buildingUnitData.GeometryMethod.Should().Be(buildingUnitWasPlanned.GeometryMethod);

            buildingUnitData.IsRemoved.Should().BeFalse();
            buildingUnitData.HasDeviation.Should().Be(buildingUnitWasPlanned.HasDeviation);

            buildingUnitData.LastEventHash.Should().Be(buildingUnitWasPlanned.GetHash());
            buildingUnitData.LastProvenanceData.Should().Be(buildingUnitWasPlanned.Provenance);
        }

        [Fact]
        public void BuildingUnitWasDeregulatedIsSavedInSnapshot()
        {
            var aggregate = new BuildingFactory(IntervalStrategy.Default, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();

            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasDeregulated = Fixture.Create<BuildingUnitWasDeregulated>();

            aggregate.Initialize(new List<object>
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                buildingUnitWasDeregulated
            });

            var snapshot = aggregate.TakeSnapshot();
            snapshot.Should().BeOfType<BuildingSnapshot>();

            var buildingSnapshot = (BuildingSnapshot)snapshot;
            var buildingUnitData = buildingSnapshot.BuildingUnits.Single(x =>
                x.BuildingUnitPersistentLocalId == buildingUnitWasPlanned.BuildingUnitPersistentLocalId);

            buildingUnitData.Function.Should().Be(buildingUnitWasPlanned.Function);
            buildingUnitData.Status.Should().Be(BuildingUnitStatus.Planned);
            buildingUnitData.AddressPersistentLocalIds.Should().BeNullOrEmpty();
            buildingUnitData.ExtendedWkbGeometry.Should().Be(buildingUnitWasPlanned.ExtendedWkbGeometry);
            buildingUnitData.GeometryMethod.Should().Be(buildingUnitWasPlanned.GeometryMethod);

            buildingUnitData.IsRemoved.Should().BeFalse();
            buildingUnitData.HasDeviation.Should().BeTrue();

            buildingUnitData.LastEventHash.Should().Be(buildingUnitWasDeregulated.GetHash());
            buildingUnitData.LastProvenanceData.Should().Be(buildingUnitWasDeregulated.Provenance);
        }
    }
}

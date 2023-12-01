namespace BuildingRegistry.Tests.AggregateTests.WhenMigratingBuilding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Building;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenNoBuildingExistsWithCommonBuildingUnits : BuildingRegistryTest
    {
        public GivenNoBuildingExistsWithCommonBuildingUnits(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void WithSingleActiveCommonBuildingUnit_ThenCommonBuildingUnitExistsOnBuilding()
        {
            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var activeCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(activeCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            sut.BuildingUnits.Should().HaveCount(2);
            sut.BuildingUnits.Should().ContainSingle(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);
        }

        [Fact]
        public void WithTwoActiveCommonBuildingUnits_ThenThrowsInvalidOperationException()
        {
            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var firstActiveCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var secondActiveCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(firstActiveCommonBuildingUnit)
                .WithBuildingUnit(secondActiveCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            Xunit.Assert.Throws<InvalidOperationException>(() =>
                sut.Initialize(new List<object>
                {
                    buildingWasMigrated
                }));
        }

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithOneActiveCommonBuildingUnitAndOneNotRealizedOrRetiredCommonBuildingUnit_ThenActiveCommonBuildingUnitExistsOnBuilding(
            string nonactiveStatus)
        {
            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var activeCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var nonactiveCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Parse(nonactiveStatus)!.Value)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(activeCommonBuildingUnit)
                .WithBuildingUnit(nonactiveCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            sut.BuildingUnits.Should().HaveCount(2);
            sut.BuildingUnits.Should().ContainSingle(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);

            var commonBuildingUnit = sut.BuildingUnits.Single(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);
            commonBuildingUnit.BuildingUnitPersistentLocalId.Should()
                .Be(new BuildingUnitPersistentLocalId(activeCommonBuildingUnit.BuildingUnitPersistentLocalId));
        }

        [Fact]
        public void WithOneCommonBuildingUnitWhichIsRemoved_ThenRemovedCommonBuildingUnitExistsOnBuilding()
        {
            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var removedCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .WithIsRemoved()
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(removedCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            sut.BuildingUnits.Should().HaveCount(2);
            sut.BuildingUnits.Should().ContainSingle(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);
        }

        [Fact]
        public void WithOneActiveCommonBuildingUnitAndOneRemovedCommonBuildingUnit_ThenActiveCommonBuildingUnitExistsOnBuilding()
        {
            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var activeCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var removedCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithStatus(BuildingUnitStatus.Planned)
                .WithIsRemoved()
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(activeCommonBuildingUnit)
                .WithBuildingUnit(removedCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            sut.BuildingUnits.Should().HaveCount(2);
            sut.BuildingUnits.Should().ContainSingle(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);

            var commonBuildingUnit = sut.BuildingUnits.Single(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);
            commonBuildingUnit.BuildingUnitPersistentLocalId.Should()
                .Be(new BuildingUnitPersistentLocalId(activeCommonBuildingUnit.BuildingUnitPersistentLocalId));
        }

        [Fact]
        public void WithTwoNonactiveCommonBuildingUnits_ThenCommonBuildingUnitWithHighestPersistentLocalIdExistsOnBuilding()
        {
            Fixture.Register(() =>
            {
                var statusses = new List<BuildingUnitStatus>
                {
                    BuildingUnitStatus.NotRealized,
                    BuildingUnitStatus.Retired,
                };

                return statusses[new Random(Fixture.Create<int>()).Next(0, statusses.Count - 1)];
            });

            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var firstNonActiveCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .Build();

            var secondNonActiveCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(firstNonActiveCommonBuildingUnit)
                .WithBuildingUnit(secondNonActiveCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            sut.BuildingUnits.Should().HaveCount(2);
            sut.BuildingUnits.Should().ContainSingle(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);

            var highestPersistentLocalId = new[]
            {
                (int)firstNonActiveCommonBuildingUnit.BuildingUnitPersistentLocalId,
                (int)secondNonActiveCommonBuildingUnit.BuildingUnitPersistentLocalId
            }.Max();

            var commonBuildingUnit = sut.BuildingUnits.Single(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);
            commonBuildingUnit.BuildingUnitPersistentLocalId.Should()
                .Be(new BuildingUnitPersistentLocalId(highestPersistentLocalId));
        }

        [Fact]
        public void WithTwoCommonBuildingUnitsWhichAreRemoved_ThenCommonBuildingUnitWithHighestPersistentLocalIdExistsOnBuilding()
        {
            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .Build();

            var firstRemovedCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithIsRemoved()
                .Build();

            var secondRemovedCommonBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithFunction(BuildingUnitFunction.Common)
                .WithIsRemoved()
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(firstRemovedCommonBuildingUnit)
                .WithBuildingUnit(secondRemovedCommonBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            sut.BuildingUnits.Should().HaveCount(2);
            sut.BuildingUnits.Should().ContainSingle(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);

            var highestPersistentLocalId = new[]
            {
                (int)firstRemovedCommonBuildingUnit.BuildingUnitPersistentLocalId,
                (int)secondRemovedCommonBuildingUnit.BuildingUnitPersistentLocalId
            }.Max();

            var commonBuildingUnit = sut.BuildingUnits.Single(x => x.Function == BuildingRegistry.Building.BuildingUnitFunction.Common);
            commonBuildingUnit.BuildingUnitPersistentLocalId.Should()
                .Be(new BuildingUnitPersistentLocalId(highestPersistentLocalId));
        }
    }
}

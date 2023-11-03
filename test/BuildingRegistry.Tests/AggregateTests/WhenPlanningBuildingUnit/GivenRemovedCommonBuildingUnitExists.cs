namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
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
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenRemovedCommonBuildingUnitExists : BuildingRegistryTest
    {
        public GivenRemovedCommonBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithNoOtherNonRemovedPlannedOrRealizedBuildingUnit_ThenNothingForCommonBuildingUnit()
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithPersistentLocalId(new BuildingUnitPersistentLocalId(789))
                .WithoutPosition()
                .WithDeviation(false);

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(123),
                    isRemoved: true)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(456),
                    BuildingUnitFunction.Common,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            command.PositionGeometryMethod,
                            buildingGeometry.Center,
                            command.Function,
                            false))));
        }

        [Theory]
        [InlineData("Planned", "Planned", "Planned")]
        [InlineData("UnderConstruction", "Planned", "Planned")]
        [InlineData("Realized", "Planned", "Realized")]
        [InlineData("Realized", "Realized", "Realized")]
        public void WithOtherBuildingUnit_ThenCommonBuildingUnitRemovalIsCorrected(
            string buildingStatus,
            string buildingUnitStatus,
            string expectedCommonBuildingUnitStatus)
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithPersistentLocalId(new BuildingUnitPersistentLocalId(3))
                .WithoutPosition()
                .WithDeviation(false);

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var commonBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(2);
            var addressPersistentLocalIds = new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(1) };

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(buildingUnitStatus)!.Value,
                    new BuildingUnitPersistentLocalId(1))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned, // Doesn't matter what the original status was
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common,
                    attachedAddresses: addressPersistentLocalIds,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            command.PositionGeometryMethod,
                            buildingGeometry.Center,
                            command.Function,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitRemovalWasCorrected(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId,
                            BuildingRegistry.Building.BuildingUnitStatus.Parse(expectedCommonBuildingUnitStatus),
                            BuildingRegistry.Building.BuildingUnitFunction.Common,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var commonBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(3);
            var addressPersistentLocalIds = new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(1) };

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(BuildingRegistry.Building.BuildingUnitStatus.Planned)!.Value,
                    new BuildingUnitPersistentLocalId(1))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned, // Doesn't matter what the original status was
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common,
                    attachedAddresses: addressPersistentLocalIds,
                    isRemoved: true)
                .Build();

            var buildingUnitWasPlanned = new BuildingUnitWasPlannedV2Builder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(2)
                .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown)
                .Build();

            var buildingUnitRemovalWasCorrected = new BuildingUnitRemovalWasCorrected(buildingPersistentLocalId,
                commonBuildingUnitPersistentLocalId,
                BuildingRegistry.Building.BuildingUnitStatus.Planned,
                BuildingRegistry.Building.BuildingUnitFunction.Common,
                Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                false);
            buildingUnitRemovalWasCorrected.SetFixtureProvenance(Fixture);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize(new object[]
            {
                buildingWasMigrated,
                buildingUnitWasPlanned,
                buildingUnitRemovalWasCorrected
            });

            // Assert
            var buildingUnit = building.BuildingUnits.First(x => x.BuildingUnitPersistentLocalId == commonBuildingUnitPersistentLocalId);

            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.LastEventHash.Should().Be(buildingUnitRemovalWasCorrected.GetHash());
            building.LastEventHash.Should().Be(buildingUnitRemovalWasCorrected.GetHash());
        }
    }
}

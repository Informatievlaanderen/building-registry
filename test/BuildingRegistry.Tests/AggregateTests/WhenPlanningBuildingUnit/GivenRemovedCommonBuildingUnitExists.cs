namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
{
    using System;
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
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingStatus = Building.BuildingStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

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
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(123),
                    isRemoved: true)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(456),
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common,
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
                    BuildingRegistry.Legacy.BuildingUnitStatus.Parse(buildingUnitStatus)!.Value,
                    new BuildingUnitPersistentLocalId(1))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned, // Doesn't matter what the original status was
                    commonBuildingUnitPersistentLocalId,
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common,
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
                            BuildingUnitStatus.Parse(expectedCommonBuildingUnitStatus),
                            BuildingUnitFunction.Common,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false,
                            addressPersistentLocalIds))));
        }
    }
}

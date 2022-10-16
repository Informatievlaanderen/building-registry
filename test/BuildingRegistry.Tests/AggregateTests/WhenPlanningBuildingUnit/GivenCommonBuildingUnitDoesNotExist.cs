namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = Building.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenCommonBuildingUnitDoesNotExist : BuildingRegistryTest
    {
        public GivenCommonBuildingUnitDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithSingleBuildingUnit_AndBuildingStatusPlanned_ThenCommonBuildingUnitWasAdded()
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithDeviation(false);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            command.PositionGeometryMethod,
                            buildingGeometry.Center,
                            command.Function,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Planned,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithSingleBuildingUnit_AndBuildingStatusUnderConstruction_ThenCommonBuildingUnitWasAdded()
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithDeviation(false);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstruction = Fixture.Create<BuildingBecameUnderConstructionV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingBecameUnderConstruction,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            command.PositionGeometryMethod,
                            buildingGeometry.Center,
                            command.Function,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Planned,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithSingleBuildingUnit_AndBuildingStatusRealized_ThenCommonBuildingUnitWasAdded()
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithDeviation(false);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = Fixture.Create<BuildingWasRealizedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingWasRealizedV2,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            command.PositionGeometryMethod,
                            buildingGeometry.Center,
                            command.Function,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Realized,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithoutBuildingUnit_AndValidBuildingStatus_ThenNoCommonBuildingUnitWasAdded()
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithDeviation(false);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            command.PositionGeometryMethod,
                            buildingGeometry.Center,
                            command.Function,
                            false))));
        }

        [Fact]
        public void WithInvalidBuildingUnits_AndValidBuildingStatus_ThenNoCommonBuildingUnitWasAdded()
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);

            var removedBuildingUnit = new BuildingUnit(
                Fixture.Create<BuildingUnitId>(),
                Fixture.Create<PersistentLocalId>(),
                BuildingUnitFunction.Unknown,
                BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<BuildingUnitPosition>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: true);

            var retiredBuildingUnit = new BuildingUnit(
                Fixture.Create<BuildingUnitId>(),
                Fixture.Create<PersistentLocalId>(),
                BuildingUnitFunction.Unknown,
                BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<BuildingUnitPosition>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: false);

            var notRealizedBuildingUnit = new BuildingUnit(
                Fixture.Create<BuildingUnitId>(),
                Fixture.Create<PersistentLocalId>(),
                BuildingUnitFunction.Unknown,
                BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<BuildingUnitPosition>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: false);

            var buildingGeometry = Fixture.Create<BuildingGeometry>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                buildingGeometry,
                isRemoved: false,
                new List<BuildingUnit>
                {
                    removedBuildingUnit,
                    retiredBuildingUnit,
                    notRealizedBuildingUnit
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasPlannedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        buildingGeometry.Center,
                        command.Function,
                        command.HasDeviation))));
        }
    }
}

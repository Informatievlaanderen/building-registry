namespace BuildingRegistry.Tests.AggregateTests.WhenMeasuringBuilding
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Datastructures;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithBuildingStatusRealized_ThenBuildingWasMeasured()
        {
            var measuredExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var command = new MeasureBuilding(
                Fixture.Create<BuildingPersistentLocalId>(),
                measuredExtendedWkbGeometry,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var firstPlannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(100);
            var secondPlannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(200);
            var notRealizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(300);
            var removedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(400);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(
                        new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                        BuildingGeometryMethod.Outlined))
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    firstPlannedBuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    secondPlannedBuildingUnitPersistentLocalId,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithBuildingUnit(BuildingUnitStatus.NotRealized, notRealizedBuildingUnitPersistentLocalId)
                .WithBuildingUnit(BuildingUnitStatus.Planned, removedBuildingUnitPersistentLocalId, isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                            command.BuildingPersistentLocalId,
                            firstPlannedBuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                            command.BuildingPersistentLocalId,
                            secondPlannedBuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasMeasured(
                            command.BuildingPersistentLocalId,
                            new []{ firstPlannedBuildingUnitPersistentLocalId },
                            new []{ secondPlannedBuildingUnitPersistentLocalId },
                            command.Geometry,
                            new BuildingGeometry(measuredExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(
                            command.BuildingPersistentLocalId,
                            command.BuildingGrbData))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("NotRealized")]
        [InlineData("UnderConstruction")]
        public void WithBuildingStatus_ThenBuildingWasRealizedAndMeasured(string buildingStatus)
        {
            var measuredExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var command = new MeasureBuilding(
                Fixture.Create<BuildingPersistentLocalId>(),
                measuredExtendedWkbGeometry,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var plannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(100);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined))
                .WithBuildingStatus(BuildingStatus.Parse(buildingStatus))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    plannedBuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasRealizedV2(
                            command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                            command.BuildingPersistentLocalId,
                            plannedBuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasMeasured(
                            command.BuildingPersistentLocalId,
                            new[] { plannedBuildingUnitPersistentLocalId },
                            Array.Empty<BuildingUnitPersistentLocalId>(),
                            command.Geometry,
                            new BuildingGeometry(measuredExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(
                            command.BuildingPersistentLocalId,
                            command.BuildingGrbData))));
        }

        [Fact]
        public void WithBuildingAlreadyMeasured_ThenNothing()
        {
            var command = Fixture.Create<MeasureBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(
                        new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                        BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void AndBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<MeasureBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: true,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Fact]
        public void WithNotPolygonGeometry_ThenInvalidPolygonExceptionWasThrown()
        {
            Fixture.Customize(new WithValidPoint());
            var command = Fixture.Create<MeasureBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Theory]
        [InlineData("Retired")]
        public void WithInvalidBuildingStatus_ThenBuildingHasInvalidStatusExceptionWasThrown(string status)
        {
            Fixture.Customize(new WithValidPoint());
            var command = Fixture.Create<MeasureBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(status),
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void StateCheck()
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var firstPlannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(100);
            var secondPlannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(200);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(
                        new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                        BuildingGeometryMethod.Outlined))
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    firstPlannedBuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    secondPlannedBuildingUnitPersistentLocalId,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .Build();

            var firstBuildingUnitWasRealized = new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    firstPlannedBuildingUnitPersistentLocalId);

            var secondBuildingUnitWasRealized = new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    secondPlannedBuildingUnitPersistentLocalId);

            var measuredExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var buildingUnitsExtendedWkbGeometry = new BuildingGeometry(measuredExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center;
            var buildingWasMeasured = new BuildingWasMeasured(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    new []{ firstPlannedBuildingUnitPersistentLocalId },
                    new []{ secondPlannedBuildingUnitPersistentLocalId },
                    measuredExtendedWkbGeometry,
                    buildingUnitsExtendedWkbGeometry);

            // Act
            building.Initialize(new object[]
            {
                buildingWasMigrated,
                firstBuildingUnitWasRealized,
                secondBuildingUnitWasRealized,
                buildingWasMeasured
            });

            // Assert
            building.BuildingStatus.Should().Be(BuildingStatus.Realized);
            building.BuildingGeometry.Geometry.Should().Be(measuredExtendedWkbGeometry);
            building.BuildingGeometry.Method.Should().Be(BuildingGeometryMethod.MeasuredByGrb);

            foreach (var buildingUnit in building.BuildingUnits)
            {
                buildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);
                buildingUnit.BuildingUnitPosition.Should().Be(
                    new BuildingUnitPosition(
                        buildingUnitsExtendedWkbGeometry,
                        BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject));
            }
        }
    }
}

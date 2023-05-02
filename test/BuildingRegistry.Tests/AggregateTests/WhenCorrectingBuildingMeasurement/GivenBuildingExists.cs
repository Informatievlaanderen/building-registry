namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingMeasurement
{
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
            var correctedExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var command = new CorrectBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                correctedExtendedWkbGeometry,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var plannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(100);
            var realizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(200);
            var notRealizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(300);
            var removedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(400);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(
                        new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                        BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    plannedBuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    realizedBuildingUnitPersistentLocalId,
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
                        new BuildingMeasurementWasCorrected(
                            command.BuildingPersistentLocalId,
                            new []{ plannedBuildingUnitPersistentLocalId },
                            new []{ realizedBuildingUnitPersistentLocalId },
                            command.Geometry,
                            new BuildingGeometry(correctedExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(
                            command.BuildingPersistentLocalId,
                            command.BuildingGrbData))));
        }

        [Fact]
        public void AndMeasurementUnchanged_ThenNothing()
        {
            var extendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary());
            var command = new CorrectBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                extendedWkbGeometry,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(
                        extendedWkbGeometry,
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

        [Theory]
        [InlineData("Planned")]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingMeasurement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(status)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void AndBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingMeasurement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Fact]
        public void AndInvalidPolygonGeometry_ThenThrowsInvalidPolygonException()
        {
            Fixture.Customize(new WithValidPoint());
            var command = Fixture.Create<CorrectBuildingMeasurement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void AndInvalidGeometryMethod_ThenThrowsBuildingHasInvalidGeometryMethodException()
        {
            var command = Fixture.Create<CorrectBuildingMeasurement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(
                    new BuildingGeometry(Fixture.Create<ExtendedWkbGeometry>(),
                        BuildingGeometryMethod.Outlined))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidGeometryMethodException()));
        }

        [Fact]
        public void StateCheck()
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var plannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(100);
            var realizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(200);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(
                    new BuildingGeometry(
                        new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                        BuildingGeometryMethod.Outlined))
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    plannedBuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    realizedBuildingUnitPersistentLocalId,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .Build();

            var measuredExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var buildingUnitsExtendedWkbGeometry = new BuildingGeometry(measuredExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center;
            var buildingMeasurementWasCorrected = new BuildingMeasurementWasCorrected(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    new []{ plannedBuildingUnitPersistentLocalId },
                    new []{ realizedBuildingUnitPersistentLocalId },
                    measuredExtendedWkbGeometry,
                    buildingUnitsExtendedWkbGeometry);

            // Act
            building.Initialize(new object[]
            {
                buildingWasMigrated,
                buildingMeasurementWasCorrected
            });

            // Assert
            building.BuildingGeometry.Geometry.Should().Be(measuredExtendedWkbGeometry);

            foreach (var buildingUnit in building.BuildingUnits)
            {
                buildingUnit.BuildingUnitPosition.Should().Be(
                    new BuildingUnitPosition(
                        buildingUnitsExtendedWkbGeometry,
                        BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject));
            }
        }
    }
}

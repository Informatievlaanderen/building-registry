namespace BuildingRegistry.Tests.AggregateTests.WhenChangingBuildingMeasurement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Datastructures;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Tests.Extensions;
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = BuildingRegistry.Legacy.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingGeometryWasChanged()
        {
            var extendedWkbGeometryBuilding = new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());

            var command = new ChangeBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                extendedWkbGeometryBuilding,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingMeasurementWasChanged(
                        command.BuildingPersistentLocalId,
                        Array.Empty<BuildingUnitPersistentLocalId>(),
                        Array.Empty<BuildingUnitPersistentLocalId>(),
                        extendedWkbGeometryBuilding,
                        null))));
        }

        [Fact]
        public void WithSameGeometry_ThenNothing()
        {
            var extendedWkbGeometry = new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary());

            var command = new ChangeBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                extendedWkbGeometry,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    extendedWkbGeometry,
                    BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithPlannedOrRealizedBuildingUnitsForWhichThePositionIsDerived_ThenBuildingUnitsPositionWasAlsoChanged()
        {
            var changedBuildingGeometry = new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.MeasuredByGrb).Center;

            var command = new ChangeBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                changedBuildingGeometry,
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(3),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(4),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingMeasurementWasChanged(
                        command.BuildingPersistentLocalId,
                        buildingUnitPersistentLocalIds: new[] { new BuildingUnitPersistentLocalId(1), new BuildingUnitPersistentLocalId(3) },
                        buildingUnitPersistentLocalIdsWhichBecameDerived: new[] { new BuildingUnitPersistentLocalId(2) },
                        changedBuildingGeometry,
                        changedBuildingUnitGeometry))));
        }

        [Fact]
        public void WhenBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = new ChangeBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithIsRemoved()
                .WithBuildingGeometry(new BuildingGeometry(
                    new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        [InlineData("UnderConstruction")]
        [InlineData("Planned")]
        public void WhenBuildingHasInvalidStatus_ThenThrowsBuildingHasInvalidStatusException(string invalidBuildingStatus)
        {
            var command = new ChangeBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(invalidBuildingStatus)
                .WithBuildingGeometry(new BuildingGeometry(
                    new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException ()));
        }

        [Fact]
        public void WhenBuildingGeometryMethodIsOutlined_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = new ChangeBuildingMeasurement(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<BuildingGrbData>(),
                Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry( new BuildingGeometry(
                    new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.Outlined))
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidGeometryMethodException()));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("Realized")]
        public void WithBuildingUnitsOutsideOfChangedBuildingGeometry_ThenThrowsBuildingUnitPositionIsOutsideBuildingGeometryException(string buildingUnitStatus)
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(buildingUnitStatus)!.Value,
                    new BuildingUnitPersistentLocalId(1),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasBuildingUnitsOutsideBuildingGeometryException()));
        }

        [Fact]
        public void StateCheck()
        {
            var changedBuildingGeometry = new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined).Center;

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(3),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(4),
                    BuildingUnitFunction.Unknown,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.ChangeOutliningConstruction(changedBuildingGeometry);

            // Assert
            sut.BuildingGeometry.Should()
                .Be(new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined));

            var plannedBuildingUnit = sut.BuildingUnits.Single(x =>
                x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(1));
            var realizedBuildingUnit = sut.BuildingUnits.Single(x =>
                x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(3));

            plannedBuildingUnit.BuildingUnitPosition.Should().Be(
                new BuildingUnitPosition(
                    changedBuildingUnitGeometry,
                    BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject));
            realizedBuildingUnit.BuildingUnitPosition.Should().Be(
                new BuildingUnitPosition(
                    changedBuildingUnitGeometry,
                    BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject));
        }
    }
}

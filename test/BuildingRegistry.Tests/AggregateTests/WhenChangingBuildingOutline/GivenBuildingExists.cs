namespace BuildingRegistry.Tests.AggregateTests.WhenChangingBuildingOutline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
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
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingOutlineWasChanged(
                        command.BuildingPersistentLocalId,
                        Array.Empty<BuildingUnitPersistentLocalId>(),
                        new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                        null))));
        }

        [Fact]
        public void WithPlannedOrRealizedBuildingUnitsForWhichThePositionIsDerived_ThenBuildingUnitsPositionWasAlsoChanged()
        {
            var changedBuildingGeometry = new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined).Center;

            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                changedBuildingGeometry,
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);
            var initialBuildingUnitGeometry =
                ExtendedWkbGeometry.CreateEWkb(new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary())
                    .ToGeometry().CentroidWithinArea().AsBinary());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        initialBuildingUnitGeometry,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        // Because this building unit's position will not change, it should (already) be inside the new building geometry.
                        new ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()),
                        BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(3),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        initialBuildingUnitGeometry,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(4),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        initialBuildingUnitGeometry,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingOutlineWasChanged(
                        command.BuildingPersistentLocalId,
                        new[] { new BuildingUnitPersistentLocalId(1), new BuildingUnitPersistentLocalId(3) },
                        changedBuildingGeometry,
                        changedBuildingUnitGeometry))));
        }

        [Fact]
        public void WithSameGeometry_ThenNothing()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WhenBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithIsRemoved()
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Fact]
        public void WhenBuildingGeometryMethodIsMeasuredByGrb_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidBuildingGeometryMethodException()));
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
                BuildingGeometryMethod.MeasuredByGrb);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(buildingUnitStatus)!.Value,
                    new BuildingUnitPersistentLocalId(1),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                        BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidBuildingGeometryMethodException()));
        }

        [Fact]
        public void StateCheck()
        {
            var changedBuildingGeometry = new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined).Center;

            var initialBuildingGeometry = new BuildingGeometry(
                new BuildingRegistry.Building.ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);
            var initialBuildingUnitGeometry =
                ExtendedWkbGeometry.CreateEWkb(new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary())
                    .ToGeometry().CentroidWithinArea().AsBinary());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        initialBuildingUnitGeometry,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        // Because this building unit's position will not change, it should (already) be inside the new building geometry.
                        new ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()),
                        BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(3),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        initialBuildingUnitGeometry,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(4),
                    BuildingUnitFunction.Unknown,
                    new BuildingUnitPosition(
                        initialBuildingUnitGeometry,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.ChangeOutlining(changedBuildingGeometry);

            // Assert
            sut.BuildingGeometry.Should()
                .Be(new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined));

            var plannedBuildingUnit = sut.BuildingUnits.Single(x =>
                x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(1));
            var realizedBuildingUnit = sut.BuildingUnits.Single(x =>
                x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(3));

            plannedBuildingUnit.BuildingUnitPosition.Should().Be(
                new BuildingRegistry.Building.BuildingUnitPosition(
                    changedBuildingUnitGeometry,
                    BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject));
            realizedBuildingUnit.BuildingUnitPosition.Should().Be(
                new BuildingRegistry.Building.BuildingUnitPosition(
                    changedBuildingUnitGeometry,
                    BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject));
        }
    }
}

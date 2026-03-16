namespace BuildingRegistry.Tests.AggregateTests.WhenChangingBuildingOutline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Handlers.Lambda;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

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
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
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
                        new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                        null))));
        }

        [Fact]
        public void WithSameGeometry_ThenNothing()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
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
        public void WithPlannedOrRealizedBuildingUnitsForWhichThePositionIsDerived_ThenBuildingUnitsPositionWasAlsoChanged()
        {
            var changedBuildingGeometry = new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon));
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined).Center;

            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                changedBuildingGeometry,
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
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
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()))
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
                    new BuildingOutlineWasChanged(
                        command.BuildingPersistentLocalId,
                        new[] { new BuildingUnitPersistentLocalId(1), new BuildingUnitPersistentLocalId(3) },
                        changedBuildingGeometry,
                        changedBuildingUnitGeometry))));
        }

        [Fact]
        public void WithPlannedOrRealizedBuildingUnitsForWhichThePositionIsAppointedByAdministrator_ThenBuildingUnitsPositionRemainsUnchanged()
        {
            var changedBuildingGeometry = new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon));
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined).Center;

            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                changedBuildingGeometry,
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(3),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()))
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
                        changedBuildingGeometry,
                        null))));
        }

        [Fact]
        public void WhenBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
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

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WhenBuildingHasInvalidStatus_ThenThrowsBuildingHasInvalidStatusException(string invalidBuildingStatus)
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                BuildingGeometryMethod.MeasuredByGrb);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(invalidBuildingStatus)
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException ()));
        }

        [Fact]
        public void WhenBuildingGeometryMethodIsMeasuredByGrb_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                BuildingGeometryMethod.MeasuredByGrb);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
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
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(buildingUnitStatus)!.Value,
                    new BuildingUnitPersistentLocalId(1),
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.PointNotInPolygon)))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasBuildingUnitsOutsideBuildingGeometryException()));
        }

        [Fact]
        public void WithPointAsGeometry_ThenInvalidPolygonExceptionWasThrown()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.PointNotInPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void WithGeometryTooSmall_ThenBuildingTooSmallExceptionWasThrown()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.TooSmallPolygon)),
                Fixture.Create<Provenance>());

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(initialBuildingGeometry)
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingOutlineIsTooSmallException()));
        }

        [Fact]
        public void WithOverlappingOutlinedBuilding_ThenThrowsBuildingGeometryOverlapsWithOutlinedBuildingException()
        {
            var command = new ChangeBuildingOutline(
                Fixture.Create<BuildingPersistentLocalId>(),
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon)),
                Fixture.Create<Provenance>());

            FakeBuildingGeometries
                .Setup(x => x.GetOverlappingBuildingOutlines(
                    It.IsAny<BuildingPersistentLocalId>(),
                    It.IsAny<ExtendedWkbGeometry>()))
                .Returns(new[]
                {
                    new BuildingGeometryData(
                        1,
                        BuildingStatus.Planned,
                        BuildingGeometryMethod.Outlined,
                        GeometryHelper.ValidPolygon,
                        false)
                });

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingGeometry(new BuildingGeometry(ExtendedWkbGeometry.CreateEWkb(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)), BuildingGeometryMethod.Outlined))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingGeometryOverlapsWithOutlinedBuildingException()));
        }

        [Fact]
        public void StateCheck()
        {
            var changedBuildingGeometry = new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.SecondValidPolygon));
            var changedBuildingUnitGeometry = new BuildingGeometry(changedBuildingGeometry, BuildingGeometryMethod.Outlined).Center;

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)),
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
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(changedBuildingUnitGeometry.ToString()))
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
            sut.ChangeOutliningConstruction(changedBuildingGeometry, new NoOverlappingBuildingGeometries());

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

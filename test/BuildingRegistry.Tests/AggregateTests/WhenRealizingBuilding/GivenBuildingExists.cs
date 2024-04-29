namespace BuildingRegistry.Tests.AggregateTests.WhenRealizingBuilding
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.BackOffice.Abstractions;
    using Api.BackOffice.Handlers.Lambda;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using BuildingRegistry.Legacy;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusUnderConstruction_ThenBuildingWasRealized()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithPlannedBuildingUnits_ThenBuildingUnitsWereRealized()
        {
            var command = Fixture.Create<RealizeBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlannedV2,
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasRealizedV2(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)))));
        }

        [Fact]
        public void WithStatusRealized_ThenDoNothing()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithStatusPlanned_ThenThrowsBuildingHasInvalidStatusException()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidStatus_ThenThrowsBuildingCannotBeRealizedException(string status)
        {
            var command = Fixture.Create<RealizeBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Parse(status))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void BuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<RealizeBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
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
        public void WithOverlappingMeasuredBuilding_ThenThrowsBuildingGeometryOverlapsWithMeasuredBuildingException()
        {
            var command = Fixture.Create<RealizeBuilding>();

            FakeBuildingGeometries
                .Setup(x => x.GetOverlappingBuildings(
                    Fixture.Create<BuildingPersistentLocalId>(),
                    It.IsAny<ExtendedWkbGeometry>()))
                .Returns(new[]
                {
                    new BuildingGeometryData(
                        1,
                        BuildingStatus.Planned,
                        BuildingGeometryMethod.MeasuredByGrb,
                        GeometryHelper.ValidPolygon,
                        false)
                });

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingGeometryOverlapsWithMeasuredBuildingException()));
        }

        [Fact]
        public void WithOverlappingOutlinedBuilding_ThenThrowsBuildingGeometryOverlapsWithOutlinedBuildingException()
        {
            var command = Fixture.Create<RealizeBuilding>();

            FakeBuildingGeometries
                .Setup(x => x.GetOverlappingBuildings(
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
            var plannedBuildingUnitPersistentLocalId = new PersistentLocalId(123);
            var retiredBuildingUnitPersistentLocalId = new PersistentLocalId(456);
            var notRealizedBuildingUnitPersistentLocalId = new PersistentLocalId(789);
            var removedBuildingUnitPersistentLocalId = new PersistentLocalId(101);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(plannedBuildingUnitPersistentLocalId)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .Build())
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(retiredBuildingUnitPersistentLocalId)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .WithStatus(BuildingUnitStatus.Retired)
                    .Build())
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(notRealizedBuildingUnitPersistentLocalId)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .WithStatus(BuildingUnitStatus.NotRealized)
                    .Build())
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(removedBuildingUnitPersistentLocalId)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithIsRemoved()
                    .Build())
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.RealizeConstruction(new NoOverlappingBuildingGeometries());

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.Realized);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));
            plannedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);

            var retiredUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(retiredBuildingUnitPersistentLocalId));
            retiredUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Retired);

            var notRealizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(notRealizedBuildingUnitPersistentLocalId));
            notRealizedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.NotRealized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));
            removedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
        }
    }
}

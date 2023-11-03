namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitNotRealization
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public partial class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithNotRealizedBuildingUnit_ThenBuildingUnitWasCorrectedToPlanned()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary());
            var buildingGeometry = new BuildingGeometry(
                buildingExtendedWkbGeometry,
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>()
                        .WithGeometry(buildingExtendedWkbGeometry),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithPosition(
                            new BuildingUnitPosition(buildingGeometry.Center,
                                BuildingUnitPositionGeometryMethod.AppointedByAdministrator)),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithPositionDerivedFromObject_ThenBuildingUnitWasCorrectedToPlannedAndPositionCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var expectedPosition = new BuildingGeometry(buildingGeometry, BuildingGeometryMethod.Outlined).Center;

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>()
                        .WithGeometry(buildingGeometry),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithPosition(
                            new BuildingUnitPosition(
                                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                                BuildingUnitPositionGeometryMethod.DerivedFromObject)),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            expectedPosition)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithPositionAppointedByAdminAndOutsideOfBuildingGeometry_ThenBuildingUnitWasCorrectedToPlannedAndPositionCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>()
                        .WithGeometry(new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary())),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithPosition(
                            new BuildingUnitPosition(
                                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                                BuildingUnitPositionGeometryMethod.AppointedByAdministrator)),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithPlannedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData("Realized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingUnitStatus_ThenThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(status)!.Value,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Parse(status))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void WithNonExistentBuildingUnit_ThenThrowsBuildingUnitNotFoundException()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsNotFoundException()));
        }

        [Fact]
        public void WithRemovedBuildingUnit_ThenThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            // Act
            building.Initialize(new object[]
            {
                Fixture.Create<BuildingWasPlannedV2>(),
                Fixture.Create<BuildingUnitWasPlannedV2>(),
                Fixture.Create<BuildingUnitWasNotRealizedV2>(),
                Fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlanned>()
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);

            var buildingUnit = building.BuildingUnits.First();
            buildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
            buildingUnit.LastEventHash.Should().NotBe(building.LastEventHash);
        }
    }
}

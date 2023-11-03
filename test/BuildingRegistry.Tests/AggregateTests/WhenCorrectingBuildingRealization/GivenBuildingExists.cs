namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingRealization
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
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusRealized_ThenBuildingWasCorrectToUnderConstruction()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasCorrectedFromRealizedToUnderConstruction(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithRealizedBuildingUnits_ThenBuildingUnitsWereCorrectedToPlanned()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var firstBuildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(1);
            var secondBuildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(2);
            var thirdBuildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(3);
            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Planned);
            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    firstBuildingUnitWasPlannedV2,
                    secondBuildingUnitWasPlannedV2,
                    commonBuildingUnitWasAddedV2,
                    thirdBuildingUnitWasPlannedV2,
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasRealizedV2>()
                        .WithBuildingUnitPersistentLocalId(
                            new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)),
                    Fixture.Create<BuildingUnitWasRealizedV2>()
                        .WithBuildingUnitPersistentLocalId(
                            new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)),
                    Fixture.Create<BuildingUnitWasRealizedV2>()
                        .WithBuildingUnitPersistentLocalId(
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasCorrectedFromRealizedToUnderConstruction(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithStatusUnderConstruction_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(Fixture.Create<BuildingGeometry>())
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                    .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                    .WithBuildingStatus(BuildingStatus.Parse(status))
                    .WithBuildingGeometry(Fixture.Create<BuildingGeometry>())
                    .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void WithBuildingGeometryMethodIsMeasuredByGrb_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>())
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.MeasuredByGrb))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidGeometryMethodException()));
        }

        [Fact]
        public void WithRetiredBuildingUnits_ThenThrowsBuildingHasRetiredBuildingUnitsException()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>())
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(BuildingRegistry.Legacy.BuildingUnitStatus.Retired)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasRetiredBuildingUnitsException()));
        }

        [Fact]
        public void StateCheck()
        {
            var plannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var realizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(2);
            var notRealizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(3);
            var removedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(4);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>())
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    plannedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    realizedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
                    notRealizedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    removedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown,
                    isRemoved: true)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.CorrectRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.UnderConstruction);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == plannedBuildingUnitPersistentLocalId);
            plannedUnit.Status.Should().Be(BuildingUnitStatus.Planned);

            var previouslyRealizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == realizedBuildingUnitPersistentLocalId);
            previouslyRealizedUnit.Status.Should().Be(BuildingUnitStatus.Planned);

            var notRealizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == notRealizedBuildingUnitPersistentLocalId);
            notRealizedUnit.Status.Should().Be(BuildingUnitStatus.NotRealized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == removedBuildingUnitPersistentLocalId);
            removedUnit.Status.Should().Be(BuildingUnitStatus.Realized);
        }
    }
}

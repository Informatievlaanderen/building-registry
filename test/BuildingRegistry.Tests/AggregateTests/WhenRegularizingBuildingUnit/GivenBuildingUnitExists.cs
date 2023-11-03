namespace BuildingRegistry.Tests.AggregateTests.WhenRegularizingBuildingUnit
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
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingUnitExists: BuildingRegistryTest
    {
        public GivenBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingUnitWasRegularized()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasRegularized(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithRegularizedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasRegularized>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithFunction(BuildingUnitFunction.Common)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void WithRemovedBuildingUnit_ThenThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithIsRemoved()
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidBuildingUnitStatus_ThenThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Parse(status)!.Value)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void BuildingStatusNotValid_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Parse(status))
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .Build())
                .Build();

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

            // Act
            building.Initialize(new object[]
            {
                Fixture.Create<BuildingWasPlannedV2>(),
                Fixture.Create<BuildingUnitWasPlannedV2>(),
                Fixture.Create<BuildingUnitWasRegularized>()
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);

            var buildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == Fixture.Create<BuildingUnitWasPlannedV2>().BuildingUnitPersistentLocalId);
            buildingUnit.HasDeviation.Should().BeFalse();
        }
    }
}

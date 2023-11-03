namespace BuildingRegistry.Tests.AggregateTests.WhenRealizingBuildingUnit
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

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithPlannedBuildingUnit_ThenBuildingUnitWasRealized()
        {
            var command = Fixture.Create<RealizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasRealizedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithRealizedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<RealizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<RealizeBuildingUnit>();

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

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidBuildingUnitStatus_ThenThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<RealizeBuildingUnit>();

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


        [Fact]
        public void WithNonExistentBuildingUnit_ThenThrowsBuildingUnitNotFoundException()
        {
            var command = Fixture.Create<RealizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
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
            var command = Fixture.Create<RealizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithFunction(BuildingUnitFunction.Unknown)
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

        [Fact]
        public void BuildingStatusNotValid_ThenThrowsBuildingStatusPreventsBuildingUnitRealizationException()
        {
            var command = Fixture.Create<RealizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .WithIsRemoved()
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
                Fixture.Create<BuildingUnitWasRealizedV2>()
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);
            var buildingUnit = building.BuildingUnits.First();
            buildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);
            buildingUnit.LastEventHash.Should().NotBe(building.LastEventHash);
        }
    }
}

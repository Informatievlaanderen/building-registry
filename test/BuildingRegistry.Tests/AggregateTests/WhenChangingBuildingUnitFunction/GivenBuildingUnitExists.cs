namespace BuildingRegistry.Tests.AggregateTests.WhenChangingBuildingUnitFunction
{
    using System.Linq;
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
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingUnitExists: BuildingRegistryTest
    {
        public GivenBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingUnitFunctionWasChanged()
        {
            var command = new ChangeBuildingUnitFunction(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingRegistry.Building.BuildingUnitFunction.Lodging,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>().WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown))
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitFunctionWasChanged(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.Function))));
        }

        [Fact]
        public void WithSameBuildingUnitFunction_ThenDoNothing()
        {
            var command = new ChangeBuildingUnitFunction(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingRegistry.Building.BuildingUnitFunction.Residential,
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>().WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Residential))
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCommonBuilding_ThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<ChangeBuildingUnitFunction>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                    .WithBuildingStatus(BuildingStatus.Planned)
                    .WithBuildingUnit(
                        BuildingUnitStatus.Planned,
                        command.BuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Common)
                    .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void BuildingUnitIsRemoved_ThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<ChangeBuildingUnitFunction>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown,
                    isRemoved: true)
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
        public void WithInvalidBuildingUnitStatus_ThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<ChangeBuildingUnitFunction>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(status)!.Value,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
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
        public void WithInvalidBuildingStatus_ThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<ChangeBuildingUnitFunction>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(status)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
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
            var building = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.IndustryBusiness);
            var buildingUnitFunctionWasChanged = Fixture.Create<BuildingUnitFunctionWasChanged>()
                .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.DancingRestaurantCafe);

            // Act
            building.Initialize(new object[]
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                buildingUnitFunctionWasChanged
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);

            var buildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitWasPlanned.BuildingUnitPersistentLocalId);
            buildingUnit.Function.Should().Be(BuildingRegistry.Building.BuildingUnitFunction.DancingRestaurantCafe);
        }
    }
}

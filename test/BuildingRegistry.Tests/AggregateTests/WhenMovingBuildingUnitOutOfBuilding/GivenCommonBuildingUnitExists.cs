namespace BuildingRegistry.Tests.AggregateTests.WhenMovingBuildingUnitOutOfBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Tests.Extensions;
    using BuildingRegistry.Tests.Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCommonBuildingUnitExists : BuildingRegistryTest
    {
        public GivenCommonBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithPlannedCommonBuildingUnitAndTwoOtherBuildingUnits_ThenCommonBuildingUnitWasNotRealized()
        {
            var command = Fixture.Create<MoveBuildingUnitOutOfBuilding>();

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Planned);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                        Fixture.Create<BuildingWasPlannedV2>(),
                        Fixture.Create<BuildingWasRealizedV2>(),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                            .WithFunction(BuildingUnitFunction.Unknown),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithFunction(BuildingUnitFunction.Unknown),
                        commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitWasMovedOutOfBuilding(
                            command.SourceBuildingPersistentLocalId,
                            command.DestinationBuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(
                            command.SourceBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))));
        }

        [Fact]
        public void WithRealizedCommonBuildingUnitAndTwoOtherBuildingUnits_ThenCommonBuildingUnitWasRetired()
        {
            var command = Fixture.Create<MoveBuildingUnitOutOfBuilding>();

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Realized);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                        .WithFunction(BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitWasMovedOutOfBuilding(
                            command.SourceBuildingPersistentLocalId,
                            command.DestinationBuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitWasRetiredV2(
                            command.SourceBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("Realized")]
        public void WithActiveCommonBuildingUnitAndThreeOtherBuildingUnits_ThenNothingForCommonBuildingUnit(string buildingUnitStatus)
        {
            var command = Fixture.Create<MoveBuildingUnitOutOfBuilding>();

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Parse(buildingUnitStatus));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                        .WithFunction(BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitWasMovedOutOfBuilding(
                            command.SourceBuildingPersistentLocalId,
                            command.DestinationBuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }
    }
}

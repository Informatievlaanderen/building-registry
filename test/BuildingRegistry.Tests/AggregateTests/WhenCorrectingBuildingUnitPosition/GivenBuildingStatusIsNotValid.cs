namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitPosition
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingStatusIsNotValid : BuildingRegistryTest
    {
        public GivenBuildingStatusIsNotValid(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void ThenBuildingHasInvalidStatusException(string buildingStatus)
        {
            var command = Fixture.Create<CorrectBuildingUnitPosition>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Parse(buildingStatus))
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }
    }
}

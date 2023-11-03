namespace BuildingRegistry.Tests.AggregateTests.WhenRetiringBuildingUnit
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

    public class GivenBuildingRemoved : BuildingRegistryTest
    {
        public GivenBuildingRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void BuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<RetireBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }
    }
}

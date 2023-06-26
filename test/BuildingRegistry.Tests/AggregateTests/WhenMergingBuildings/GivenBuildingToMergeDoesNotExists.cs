namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenBuildingToMergeDoesNotExists : BuildingRegistryTest
    {
        public GivenBuildingToMergeDoesNotExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenAggregateNotFoundExceptionWasThrown()
        {
            var command = Fixture.Create<MergeBuildings>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new AggregateNotFoundException(
                    new BuildingStreamId(command.BuildingPersistentLocalIdsToMerge.First()),
                    typeof(Building))));
        }
    }
}

namespace BuildingRegistry.Tests.AggregateTests.WhenRequestingBuildingSnapshot
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoBuildingExists : BuildingRegistryTest
    {
        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowsAggregateNotFoundException()
        {
            var command = Fixture.Create<CreateSnapshot>();
            var streamId = new BuildingStreamId(command.BuildingPersistentLocalId);

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new AggregateNotFoundException(streamId, typeof(Building))));
        }
    }
}

namespace BuildingRegistry.Tests.AggregateTests.WhenMarkingBuildingAsMerged
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingDoesNotExist : BuildingRegistryTest
    {
        public GivenBuildingDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenThrowsAggregateNotFoundException()
        {
            var command = new MarkBuildingAsMerged(Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new AggregateNotFoundException(new BuildingStreamId(command.BuildingPersistentLocalId), typeof(Building))));
        }
    }
}

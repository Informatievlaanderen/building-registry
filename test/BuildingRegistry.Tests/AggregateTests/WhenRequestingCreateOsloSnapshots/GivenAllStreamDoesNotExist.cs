namespace BuildingRegistry.Tests.AggregateTests.WhenRequestingCreateOsloSnapshots
{
    using AllStream;
    using AllStream.Commands;
    using AllStream.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAllStreamDoesNotExist : BuildingRegistryTest
    {
        public GivenAllStreamDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenBuildingUnitOsloSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [],
                [new BuildingUnitPersistentLocalId(1)],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .When(command)
                .Then(AllStreamId.Instance,
                    new BuildingUnitOsloSnapshotsWereRequested(
                        command.BuildingUnitPersistentLocalIds)));
        }
    }
}

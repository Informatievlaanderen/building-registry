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

    public class GivenParcelsExist : BuildingRegistryTest
    {
        public GivenParcelsExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenParcelOsloSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [new BuildingUnitPersistentLocalId(1)],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(AllStreamId.Instance)
                .When(command)
                .Then(AllStreamId.Instance,
                    new BuildingUnitOsloSnapshotsWereRequested(
                        command.BuildingUnitPersistentLocalIds)));
        }
    }
}

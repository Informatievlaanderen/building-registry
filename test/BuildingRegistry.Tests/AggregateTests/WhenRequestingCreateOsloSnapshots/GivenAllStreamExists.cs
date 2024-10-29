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

    public class GivenAllStreamExists : BuildingRegistryTest
    {
        public GivenAllStreamExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenBuildingOsloSnapshotsAndBuildingUnitSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [new BuildingPersistentLocalId(1)],
                [new BuildingUnitPersistentLocalId(2)],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(AllStreamId.Instance)
                .When(command)
                .Then(AllStreamId.Instance,
                    new BuildingOsloSnapshotsWereRequested(command.BuildingPersistentLocalIds),
                    new BuildingUnitOsloSnapshotsWereRequested(command.BuildingUnitPersistentLocalIds)));
        }

        [Fact]
        public void WithOnlyBuildingUnits_ThenBuildingUnitOsloSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [],
                [new BuildingUnitPersistentLocalId(1)],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(AllStreamId.Instance)
                .When(command)
                .Then(AllStreamId.Instance,
                    new BuildingUnitOsloSnapshotsWereRequested(
                        command.BuildingUnitPersistentLocalIds)));
        }

        [Fact]
        public void WithOnlyBuildings_ThenBuildingOsloSnapshotsWereRequested()
        {
            var command = new CreateOsloSnapshots(
                [new BuildingPersistentLocalId(1)],
                [],
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(AllStreamId.Instance)
                .When(command)
                .Then(AllStreamId.Instance,
                    new BuildingOsloSnapshotsWereRequested(
                        command.BuildingPersistentLocalIds)));
        }
    }
}

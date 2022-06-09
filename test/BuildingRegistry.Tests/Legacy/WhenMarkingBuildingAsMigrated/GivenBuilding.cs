namespace BuildingRegistry.Tests.Legacy.WhenMarkingBuildingAsMigrated
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithInfiniteLifetime());
        }

        [Fact]
        public void ThenBuildingWasMarkedAsMigrated()
        {
            var buildingId = _fixture.Create<BuildingId>();

            var command = _fixture.Create<MarkBuildingAsMigrated>();

            var buildingWasMarkedAsMigrated = new BuildingWasMarkedAsMigrated(buildingId, command.PersistentLocalId);
            ((ISetProvenance)buildingWasMarkedAsMigrated).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    buildingWasMarkedAsMigrated));
        }
    }
}

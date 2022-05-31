namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRemoved : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithInfiniteLifetime());
        }

        [Fact]
        public void ThenBuildingRemovedExceptionIsThrown()
        {
            _fixture.Customize(new WithNoDeleteModification());

            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRemoved>()
                        .WithNoUnits())
                .When(command)
                .Throws(new BuildingRemovedException()));
        }
    }
}

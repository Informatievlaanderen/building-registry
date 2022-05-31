namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObject
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRemovedBuilding : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenRemovedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
        }

        [Fact]
        public void ThenExceptionIsThrown()
        {
            _fixture.Customize(new WithNoDeleteModification());
            var command = _fixture.Create<ImportTerrainObjectFromCrab>();

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

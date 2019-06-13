namespace BuildingRegistry.Tests.WhenImportingCrabSubaddress
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Autofixture;
    using AutoFixture;
    using Building;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRemoved : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
        }

        [Fact]
        public void ThenBuildingRemovedExceptionIsThrown()
        {
            _fixture.Customize(new WithNoDeleteModification());

            var command = _fixture.Create<ImportSubaddressFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRemoved>()
                        .WithNoUnits())
                .When(command)
                .ThenNone()); //because we can receive subaddress addeds (to houseNumber) even if the relation between both has been deleted.
        }
    }
}

namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabHouseNumberPosition
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithUnitWithNoGeometry : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingWithUnitWithNoGeometry(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
            _fixture.Customize(new WithValidPoint());
        }

        [Fact]
        public void WithValidGeometry()
        {
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));

        }
    }
}

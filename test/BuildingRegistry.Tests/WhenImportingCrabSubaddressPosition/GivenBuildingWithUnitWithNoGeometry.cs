namespace BuildingRegistry.Tests.WhenImportingCrabSubaddressPosition
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithUnitWithNoGeometry : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingWithUnitWithNoGeometry(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
            _fixture.Customize(new WithValidPoint());
        }

        [Fact]
        public void WithValidGeometry()
        {
            var command = _fixture.Create<ImportSubaddressPositionFromCrab>();
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

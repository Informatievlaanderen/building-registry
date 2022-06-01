namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabSubaddressStatus
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsRemoved : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnitIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
        }

        [Fact]
        public void ThenLegacyEventIsApplied()
        {
            _fixture.Customize(new WithNoDeleteModification());

            var command = _fixture.Create<ImportSubaddressStatusFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRemoved>())
                .When(command)
                .Then(buildingId, command.ToLegacyEvent()));
        }
    }
}

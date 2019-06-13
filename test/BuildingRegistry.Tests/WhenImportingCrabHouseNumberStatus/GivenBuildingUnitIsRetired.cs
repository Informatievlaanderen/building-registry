namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberStatus
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsRetired : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnitIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ThenNothingHappens(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRetired>())
                .When(importStatus)
                .Then(buildingId,
                   importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ThenStatusIsChangedToNotRealized(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRetired>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }
    }
}

namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabHouseNumberStatus
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsNotRealized : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnitIsNotRealized(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ThenNothingHappens(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasNotRealized>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ByParent_ThenNothingHappens(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasNotRealizedByParent>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ButRetiredByBuilding_ThenBecomesNotRealized(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAddedToRetiredBuilding>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ThenBecomesRetired(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasNotRealized>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ByParent_ThenBecomesRetired(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasNotRealizedByParent>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ButRetiredByBuilding_ThenNothingHappens(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAddedToRetiredBuilding>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }
    }
}

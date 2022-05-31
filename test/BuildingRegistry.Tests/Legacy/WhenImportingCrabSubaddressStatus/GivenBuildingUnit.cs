namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabSubaddressStatus
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

    public class GivenBuildingUnit : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Proposed)]
        [InlineData(CrabAddressStatus.Reserved)]
        public void WhenStatusMapsToPlanned(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(status);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasPlanned(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithEmptyStatusWhenModificationRemoved()
        {
            var importStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusProposedWhenStatusIsRealizedAndNewerLifetime()
        {
            var importedStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Proposed)
                .WithLifetime(new CrabLifetime(importedStatus.Lifetime.BeginDateTime.Value.PlusDays(1), importedStatus.Lifetime.EndDateTime));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRealized>(),
                    importedStatus.ToLegacyEvent())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasPlanned(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusProposedWhenStatusIsRealizedAndOlderLifetime()
        {
            var importedStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Proposed)
                .WithLifetime(new CrabLifetime(importedStatus.Lifetime.BeginDateTime.Value.PlusDays(-1), importedStatus.Lifetime.EndDateTime));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRealized>(),
                    importedStatus.ToLegacyEvent())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }
    }
}

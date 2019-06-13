namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberStatus
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitWithStatus_Corrections : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnitWithStatus_Corrections(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WithStatusPlannedWhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status)
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasPlanned>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasCorrectedToRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WithStatusRealizedWhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status)
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasCorrectedToRealized>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void WithStatusRealizedWhenStatusMapsToPlanned(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status)
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRealized>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasCorrectedToPlanned(buildingId, _fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void WithStatusPlannedWhenStatusMapsToPlanned(CrabAddressStatus status)
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status)
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasCorrectedToPlanned>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }
    }
}

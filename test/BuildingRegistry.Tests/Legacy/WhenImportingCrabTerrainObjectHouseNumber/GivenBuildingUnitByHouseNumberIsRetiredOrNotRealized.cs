namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using NodaTime;
    using WhenImportingCrabHouseNumberStatus;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitByHouseNumberIsRetiredOrNotRealized : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnitByHouseNumberIsRetiredOrNotRealized(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void ThenCreateNewBuildingUnit()
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId,
                importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);

            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())),
                    _fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent(),
                    _fixture.Create<BuildingUnitWasRetired>())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, expectedUnitId, buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumber.Timestamp), BuildingUnitId.Create(buildingUnitKey, 1)),
                    new BuildingUnitWasRealized(buildingId, expectedUnitId),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }
    }
}

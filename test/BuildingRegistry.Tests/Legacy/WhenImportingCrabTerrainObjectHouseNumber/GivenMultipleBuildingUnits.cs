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
    using Xunit;
    using Xunit.Abstractions;

    public class GivenMultipleBuildingUnits : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();
        public GivenMultipleBuildingUnits(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void WithFiniteLifetime()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();


            var buildingUnitKey1 = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitKey = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    buildingUnitWasAdded
                        .WithBuildingUnitId(BuildingUnitId.Create(buildingUnitKey1, 1))
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitWasRetired(buildingId, commonBuildingUnitId),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }
    }
}

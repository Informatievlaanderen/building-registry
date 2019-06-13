namespace BuildingRegistry.Tests.WhenImportingCrabSubaddress
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsRetired : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ImportTerrainObjectHouseNumberFromCrab _importTerrainObjectHouseNumberFromCrab;
        private readonly BuildingUnitWasAdded _buildingUnitWasAddedHnr;

        public GivenBuildingUnitIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());

            _importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_fixture.Create<ImportSubaddressFromCrab>().TerrainObjectHouseNumberId);

            var houseNrKey = BuildingUnitKey.Create(_importTerrainObjectHouseNumberFromCrab.TerrainObjectId, _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId);
            _buildingUnitWasAddedHnr = new BuildingUnitWasAdded(_fixture.Create<BuildingId>(), BuildingUnitId.Create(houseNrKey, 1), houseNrKey, AddressId.CreateFor(_importTerrainObjectHouseNumberFromCrab.HouseNumberId), new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));
            ((ISetProvenance)_buildingUnitWasAddedHnr).SetProvenance(_fixture.Create<Provenance>());
        }

        [Fact]
        public void WhenModificationDelete()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, _fixture.Create<BuildingUnitId>(), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitWasRetired = new BuildingUnitWasRetired(buildingId, BuildingUnitId.Create(buildingUnitKey, 1));
            ((ISetProvenance)buildingUnitWasRetired).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    buildingUnitWasRetired)
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, _fixture.Create<BuildingUnitId>()),
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithBuildingUnitWhenModificationDelete()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, _fixture.Create<BuildingUnitId>(), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitWasRetired = new BuildingUnitWasRetired(buildingId, BuildingUnitId.Create(buildingUnitKey, 1));
            ((ISetProvenance)buildingUnitWasRetired).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitId2 = BuildingUnitId.Create(buildingUnitKey, 2);
            var buildingUnitWasAdded2 = new BuildingUnitWasAdded(buildingId, buildingUnitId2, buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded2).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    buildingUnitWasRetired,
                    buildingUnitWasAdded2)
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, buildingUnitId2),
                    new BuildingUnitWasRemoved(buildingId, _fixture.Create<BuildingUnitId>()),
                    importSubaddress.ToLegacyEvent()));
        }
    }
}

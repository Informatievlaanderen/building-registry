namespace BuildingRegistry.Tests.WhenImportingCrabSubaddress
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using System;
    using ValueObjects;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnit : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly ImportTerrainObjectHouseNumberFromCrab _importTerrainObjectHouseNumberFromCrab;
        private readonly BuildingUnitWasAdded _buildingUnitWasAddedHnr;

        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
        public void WhenBuildingUnitByHouseNumberThenBuildingUnitIsAdded()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId);
            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId), AddressId.CreateFor(importSubaddress.HouseNumberId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var commonBuildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId);
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent())
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1),
                        buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new CommonBuildingUnitWasAdded(buildingId, BuildingUnitId.Create(commonBuildingUnitKey, 1), commonBuildingUnitKey, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasRealized(buildingId, BuildingUnitId.Create(commonBuildingUnitKey, 1)),
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeWhenBuildingUnitByHouseNumberThenBuildingUnitIsAddedAndRetired()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId), AddressId.CreateFor(importSubaddress.HouseNumberId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent())
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, buildingUnitId,
                        buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasNotRealized(buildingId, buildingUnitId),
                    new BuildingUnitAddressWasDetached(buildingId, AddressId.CreateFor(importSubaddress.SubaddressId), buildingUnitId),
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsRealizedWhenLifetimeIsFinite()
        {
            var importStatus = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    _fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), _fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        /// <see cref="GivenBuildingUnitByHouseNumber"></see> & <see cref="GivenBuildingUnitByHouseNumberIsRetiredOrNotRealized"/> for other tests (same behavior)

        [Fact]
        public void WhenSameBuildingUnit()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId);
            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId), AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded)
                .When(importSubaddress)
                .Then(buildingId,
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithDelete()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, _fixture.Create<BuildingUnitId>(), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId), AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded)
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, _fixture.Create<BuildingUnitId>()),
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithSameUnitIsRetired()
        {
            var importSubaddress = _fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId);
            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitWasRetired = new BuildingUnitWasRetired(buildingId, BuildingUnitId.Create(buildingUnitKey, 1));
            ((ISetProvenance)buildingUnitWasRetired).SetProvenance(_fixture.Create<Provenance>());

            var commonUnitKey = BuildingUnitKey.Create(new CrabTerrainObjectId(buildingUnitKey.Building));
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    buildingUnitWasRetired)
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 2), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp), BuildingUnitId.Create(buildingUnitKey, 1)),
                    new CommonBuildingUnitWasAdded(buildingId, BuildingUnitId.Create(commonUnitKey, 1), commonUnitKey, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasRealized(buildingId, BuildingUnitId.Create(commonUnitKey, 1)),
                    importSubaddress.ToLegacyEvent()));
        }
    }
}

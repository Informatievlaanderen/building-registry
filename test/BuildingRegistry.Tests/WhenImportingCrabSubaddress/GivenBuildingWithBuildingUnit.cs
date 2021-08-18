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
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building.DataStructures;
    using Building.Events.Crab;
    using ValueObjects;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnit : SnapshotBasedTest
    {
        private readonly ImportTerrainObjectHouseNumberFromCrab _importTerrainObjectHouseNumberFromCrab;
        private readonly BuildingUnitWasAdded _buildingUnitWasAddedHnr;

        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());

            _importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(Fixture.Create<ImportSubaddressFromCrab>().TerrainObjectHouseNumberId);

            var houseNrKey = BuildingUnitKey.Create(_importTerrainObjectHouseNumberFromCrab.TerrainObjectId, _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId);
            _buildingUnitWasAddedHnr = new BuildingUnitWasAdded(Fixture.Create<BuildingId>(), BuildingUnitId.Create(houseNrKey, 1), houseNrKey, AddressId.CreateFor(_importTerrainObjectHouseNumberFromCrab.HouseNumberId), new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));
            ((ISetProvenance)_buildingUnitWasAddedHnr).SetProvenance(Fixture.Create<Provenance>());
        }

        [Fact]
        public void WhenBuildingUnitByHouseNumberThenBuildingUnitIsAdded()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId);
            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId), AddressId.CreateFor(importSubaddress.HouseNumberId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var commonBuildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId);
            var buildingUnitWasAdded2 = new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(buildingId, BuildingUnitId.Create(commonBuildingUnitKey, 1), commonBuildingUnitKey, new BuildingUnitVersion(importSubaddress.Timestamp));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent())
                .When(importSubaddress)
                .Then(new Fact[]
                    {
                        new Fact(buildingId, buildingUnitWasAdded2),
                        new Fact(buildingId, commonBuildingUnitWasAdded),
                        new Fact(buildingId, new BuildingUnitWasRealized(buildingId, BuildingUnitId.Create(commonBuildingUnitKey, 1))),
                        new Fact(buildingId, importSubaddress.ToLegacyEvent()),
                        new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithLastModificationFromCrab(Modification.Update)
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                            {
                                _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId
                            })
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId, _importTerrainObjectHouseNumberFromCrab.HouseNumberId }
                            })
                            .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                            {
                                { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(importSubaddress.TerrainObjectHouseNumberId, importSubaddress.HouseNumberId), new List<AddressSubaddressWasImportedFromCrab>{importSubaddress.ToLegacyEvent()} }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded2),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded)
                                        .WithStatus(BuildingUnitStatus.Realized)
                                }))
                            .Build(6, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WithFiniteLifetimeWhenBuildingUnitByHouseNumberThenBuildingUnitIsAddedAndRetired()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId), AddressId.CreateFor(importSubaddress.HouseNumberId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
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
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, Fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), Fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        /// <see cref="GivenBuildingUnitByHouseNumber"></see> & <see cref="GivenBuildingUnitByHouseNumberIsRetiredOrNotRealized"/> for other tests (same behavior)

        [Fact]
        public void WhenSameBuildingUnit()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId);
            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId), AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
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
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, Fixture.Create<BuildingUnitId>(), BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId), AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded)
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, Fixture.Create<BuildingUnitId>()),
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithSameUnitIsRetired()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId);
            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasRetired = new BuildingUnitWasRetired(buildingId, BuildingUnitId.Create(buildingUnitKey, 1));
            ((ISetProvenance)buildingUnitWasRetired).SetProvenance(Fixture.Create<Provenance>());

            var commonUnitKey = BuildingUnitKey.Create(new CrabTerrainObjectId(buildingUnitKey.Building));
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
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

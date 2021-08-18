namespace BuildingRegistry.Tests.WhenImportingCrabSubaddress
{
    using System;
    using System.Collections.Generic;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using ValueObjects;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsRetired : SnapshotBasedTest
    {
        private readonly ImportTerrainObjectHouseNumberFromCrab _importTerrainObjectHouseNumberFromCrab;
        private readonly BuildingUnitWasAdded _buildingUnitWasAddedHnr;

        public GivenBuildingUnitIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
        public void WhenModificationDelete()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, Fixture.Create<BuildingUnitId>(), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasRetired = new BuildingUnitWasRetired(buildingId, BuildingUnitId.Create(buildingUnitKey, 1));
            ((ISetProvenance)buildingUnitWasRetired).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    buildingUnitWasRetired)
                .When(importSubaddress)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, Fixture.Create<BuildingUnitId>()),
                    importSubaddress.ToLegacyEvent()));
        }

        [Fact]
        public void WithBuildingUnitWhenModificationDelete()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithHouseNumberId(_importTerrainObjectHouseNumberFromCrab.HouseNumberId)
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importSubaddress.TerrainObjectId, importSubaddress.TerrainObjectHouseNumberId, importSubaddress.SubaddressId);
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, Fixture.Create<BuildingUnitId>(), buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasRetired = new BuildingUnitWasRetired(buildingId, BuildingUnitId.Create(buildingUnitKey, 1));
            ((ISetProvenance)buildingUnitWasRetired).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitId2 = BuildingUnitId.Create(buildingUnitKey, 2);
            var buildingUnitWasAdded2 = new BuildingUnitWasAdded(buildingId, buildingUnitId2, buildingUnitKey, AddressId.CreateFor(importSubaddress.SubaddressId), new BuildingUnitVersion(importSubaddress.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    _buildingUnitWasAddedHnr,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(),
                    buildingUnitWasAdded,
                    buildingUnitWasRetired,
                    buildingUnitWasAdded2)
                .When(importSubaddress)
                .Then(new Fact[]
                {
                    new Fact(buildingId, new BuildingUnitWasRemoved(buildingId, buildingUnitId2)),
                    new Fact(buildingId, new BuildingUnitWasRemoved(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, importSubaddress.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId, _importTerrainObjectHouseNumberFromCrab.HouseNumberId }
                        })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId, _importTerrainObjectHouseNumberFromCrab.HouseNumberId), new List<AddressSubaddressWasImportedFromCrab>{ importSubaddress.ToLegacyEvent() }}
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAddedHnr),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Retired)
                                    .WithRemoved(),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded2)
                                    .WithRemoved()
                            }))
                        .Build(8, EventSerializerSettings))
                }));
        }
    }
}

namespace BuildingRegistry.Tests.WhenReaddressingSubaddress
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
    using NodaTime;
    using ValueObjects;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRetiredReaddressedSubaddress : SnapshotBasedTest
    {
        #region Snapshot variables

        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumberFromCrab;
        private BuildingUnitWasAdded? _buildingUnitWasAdded;
        private ImportSubaddressFromCrab? _importSubaddress;
        private BuildingUnitWasAdded? _buildingUnit2WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress;
        private ImportSubaddressFromCrab? _importNewSubaddress;
        private ImportSubaddressFromCrab? _importRetireNewSubaddressFromCrab;
        private BuildingUnitWasReaddressed? _buildingUnitWasReaddressed;
        private ImportSubaddressFromCrab? _importNewSubaddressAgainFromCrab;
        private BuildingUnitWasAdded? _buildingUnit2WasAddedV2;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAddedV2;

        #endregion

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
        private CrabHouseNumberId HuisNr16Id { get; }
        public CrabSubaddressId OldSubaddressNr16Bus1Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);
        public BuildingUnitKey OldGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, OldSubaddressNr16Bus1Id);
        public BuildingUnitKey NewGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, NewSubaddressNr16Bus1Id);
        public BuildingUnitKey GebouwEenheid3Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(OldGebouwEenheid2Key, 1);
        private BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(NewGebouwEenheid2Key, 2);
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
        public BuildingUnitId GebouwEenheid3Idv2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);

        private AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);

        public AddressId OldAddress16Bus1Id => AddressId.CreateFor(OldSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);

        public GivenRetiredReaddressedSubaddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            HuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            HuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            OldSubaddressNr16Bus1Id = new CrabSubaddressId(161);
            NewSubaddressNr16Bus1Id = new CrabSubaddressId(163);
            ReaddressingBeginDate = new ReaddressingBeginDate(Fixture.Create<LocalDate>());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            _importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id);

            _buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(_importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    _buildingUnitWasAdded,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            _importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            _buildingUnit2WasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(_importSubaddress.Timestamp));
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(_importSubaddress.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(_importSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnit2WasAdded,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    _importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            _importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            _buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(_importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnitWasReaddressed,
                    _importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedSubaddressWithNewSubaddressId()
        {
            _importNewSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(_importNewSubaddress)
                .Then(Gebouw1Id,
                    _importNewSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireWithNewSubaddressId()
        {
            _importRetireNewSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressedSubaddressWithNewSubaddressId())
                .When(_importRetireNewSubaddressFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Bus1Id, GebouwEenheid2Id),
                    new BuildingUnitWasRetired(Gebouw1Id, GebouwEenheid3Id),
                    _importRetireNewSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ReaddSubaddressWithNewSubaddressId()
        {
            _importNewSubaddressAgainFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus1Id);

            _buildingUnit2WasAddedV2 = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid4Id, NewGebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(_importNewSubaddressAgainFromCrab.Timestamp), GebouwEenheid2Id);
            _commonBuildingUnitWasAddedV2 = new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Idv2, GebouwEenheid3Key, new BuildingUnitVersion(_importNewSubaddressAgainFromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(RetireWithNewSubaddressId())
                .When(_importNewSubaddressAgainFromCrab)
                .Then(Gebouw1Id,
                    _buildingUnit2WasAddedV2,
                    _commonBuildingUnitWasAddedV2,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Idv2),
                    _importNewSubaddressAgainFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusWithNewSubaddressId()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Reserved);

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddSubaddressWithNewSubaddressId())
                .When(importStatus)
                .Then(
                    new Fact(Gebouw1Id, new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid4Id)),
                    new Fact(Gebouw1Id, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(Gebouw1Id)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> { { HuisNr16KoppelingId, HuisNr16Id } })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId> { HuisNr16KoppelingId })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(HuisNr16KoppelingId, HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress.ToLegacyEvent(), _importNewSubaddress.ToLegacyEvent(), _importRetireNewSubaddressFromCrab.ToLegacyEvent(), _importNewSubaddressAgainFromCrab.ToLegacyEvent() } }
                        })
                        .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                        {
                            { OldGebouwEenheid2Key, _importReaddressingSubaddress.ToLegacyEvent() }
                        })
                        .WithSubaddressStatusEventsBySubaddressId(new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>
                        {
                            { importStatus.SubaddressId, new List<AddressSubaddressStatusWasImportedFromCrab>{importStatus.ToLegacyEvent()} }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(NewAddress16Bus1Id)
                                    .WithReaddressedEvents(_buildingUnitWasReaddressed)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithRetiredBySelf(),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Retired),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAddedV2)
                                    .WithAddressIds(new List<AddressId>{NewAddress16Bus1Id})
                                    .WithStatus(BuildingUnitStatus.Planned)
                                    .WithSubaddressStatusChronicle(new List<AddressSubaddressStatusWasImportedFromCrab>{importStatus.ToLegacyEvent()}),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAddedV2)
                                    .WithStatus(BuildingUnitStatus.Realized)
                            })
                            .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey> { { NewGebouwEenheid2Key, OldGebouwEenheid2Key } }))

                        .Build(19, EventSerializerSettings))
                    );
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddSubaddressUnitTest()
        {
            Assert(AddSubaddress());
        }

        [Fact]
        public void AddReaddressingSubaddressTest()
        {
            Assert(AddReaddressingOfSubaddress());
        }

        [Fact]
        public void AddReaddressedSubaddressWithNewSubaddressIdTest()
        {
            Assert(AddReaddressedSubaddressWithNewSubaddressId());
        }

        [Fact]
        public void RetireNewHouseNumberTest()
        {
            Assert(RetireWithNewSubaddressId());
        }

        [Fact]
        public void ReaddRetiredHouseNumberTest()
        {
            Assert(ReaddSubaddressWithNewSubaddressId());
        }

        [Fact]
        public void SetStatusForNewBuildingUnit()
        {
            Assert(SetStatusWithNewSubaddressId());
        }
    }
}

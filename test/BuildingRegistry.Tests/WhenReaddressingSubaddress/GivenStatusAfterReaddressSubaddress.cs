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

    public class GivenStatusAfterReaddressSubaddress : SnapshotBasedTest
    {
        #region Snapshot variables

        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumberFromCrab;
        private BuildingUnitWasAdded? _buildingUnitWasAdded;
        private ImportSubaddressFromCrab? _importSubaddress;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit2WasAdded;
        private ImportSubaddressStatusFromCrab? _importSubaddressStatus;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress;
        private ImportSubaddressStatusFromCrab? _importNewSubaddressStatus;
        private ImportSubaddressStatusFromCrab? _importOldSubaddressStatus;
        private BuildingUnitWasReaddressed? _buildingUnitWasReaddressed;

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
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);

        private AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);

        public AddressId OldAddress16Bus1Id => AddressId.CreateFor(OldSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);

        public GivenStatusAfterReaddressSubaddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

        public IEventCentricTestSpecificationBuilder SetSubaddressStatus()
        {
            _importSubaddressStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Reserved)
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(-1)), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(_importSubaddressStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid2Id),
                    _importSubaddressStatus.ToLegacyEvent());
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
                .Given(SetSubaddressStatus())
                .When(_importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnitWasReaddressed,
                    _importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSubaddressStatusWithNewSubaddressId()
        {
            _importNewSubaddressStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(_importNewSubaddressStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid2Id),
                    _importNewSubaddressStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusWithOldSubaddressId()
        {
            _importOldSubaddressStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Reserved)
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(SetSubaddressStatusWithNewSubaddressId())
                .When(_importOldSubaddressStatus)
                .Then(Gebouw1Id,
                    _importOldSubaddressStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSameStatusWithNewSubaddressIdButBeforeSetOldStatus()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importSubaddressStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(SetStatusWithOldSubaddressId())
                .When(importSubaddressStatus)
                .Then(new Fact(Gebouw1Id, importSubaddressStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(Gebouw1Id)
                            .WithLastModificationFromCrab(Modification.Update)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> { { HuisNr16KoppelingId, HuisNr16Id } })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId> { HuisNr16KoppelingId })
                            .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                            {
                                { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(HuisNr16KoppelingId, HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress.ToLegacyEvent() } }
                            })
                            .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                            {
                                { OldGebouwEenheid2Key, _importReaddressingSubaddress.ToLegacyEvent() }
                            })
                            .WithSubaddressStatusEventsBySubaddressId(new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>
                            {

                                { _importSubaddressStatus.SubaddressId, new List<AddressSubaddressStatusWasImportedFromCrab>{_importSubaddressStatus.ToLegacyEvent()} },
                                { _importNewSubaddressStatus.SubaddressId, new List<AddressSubaddressStatusWasImportedFromCrab>{_importNewSubaddressStatus.ToLegacyEvent(), importSubaddressStatus.ToLegacyEvent()} }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                        .WithAddressIds(new List<AddressId>{NewAddress16Bus1Id})
                                        .WithReaddressedEvents(_buildingUnitWasReaddressed)
                                        .WithStatus(BuildingUnitStatus.Realized)
                                        .WithSubaddressStatusChronicle(new List<AddressSubaddressStatusWasImportedFromCrab>
                                        {
                                            _importSubaddressStatus.ToLegacyEvent(),
                                            _importNewSubaddressStatus.ToLegacyEvent(),
                                            importSubaddressStatus.ToLegacyEvent()
                                        }),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                        .WithStatus(BuildingUnitStatus.Realized),

                                })
                                .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey> { { NewGebouwEenheid2Key, OldGebouwEenheid2Key } }))

                            .Build(14, EventSerializerSettings)));
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
        public void SetStatusTest()
        {
            Assert(SetSubaddressStatus());
        }

        [Fact]
        public void AddReaddressingSubaddressTest()
        {
            Assert(AddReaddressingOfSubaddress());
        }

        [Fact]
        public void SetStatusWithNewSubaddressIdTest()
        {
            Assert(SetSubaddressStatusWithNewSubaddressId());
        }

        [Fact]
        public void AddReaddressedOldSubaddressStatusWithOldSubaddressTest()
        {
            Assert(SetStatusWithOldSubaddressId());
        }

        [Fact]
        public void SetSameStatusWithNewSubaddressIdButBeforeSetOldStatusTest()
        {
            Assert(SetSameStatusWithNewSubaddressIdButBeforeSetOldStatus());
        }
    }
}

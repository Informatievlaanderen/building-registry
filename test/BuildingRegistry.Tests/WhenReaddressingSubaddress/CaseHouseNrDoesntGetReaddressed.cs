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

    public class CaseHouseNrDoesntGetReaddressed : SnapshotBasedTest
    {
        private LocalDate _readdressingBeginDate;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress;
        private ImportSubaddressFromCrab? _importSubaddress;
        private ImportSubaddressFromCrab? _importNewSubaddress1;
        private ImportSubaddressFromCrab? _importNewSubaddress2;
        private BuildingUnitWasAdded? _buildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit2WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit3WasAdded;
        private BuildingUnitWasAdded? _buildingUnit4WasAdded;
        private BuildingUnitWasReaddressed? _buildingUnitWasReaddressed;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId HuisNr16Id { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }
        public CrabSubaddressId OldSubaddressNr16Bus1Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus2Id { get; }
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
        public BuildingUnitKey GebouwEenheid4Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId, NewSubaddressNr16Bus2Id);

        private BuildingUnitKey NewHouseNrKey =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId NewGebouweenheidHouseNrId => BuildingUnitId.Create(NewHouseNrKey, 1);
        private BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(OldGebouwEenheid2Key, 1);
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
        public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);

        private AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);

        public AddressId OldAddress16Bus1Id => AddressId.CreateFor(OldSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus2Id => AddressId.CreateFor(NewSubaddressNr16Bus2Id);

        public CaseHouseNrDoesntGetReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            HuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
            HuisNr16Id = new CrabHouseNumberId(1616);
            NewHuisNr16Id = new CrabHouseNumberId(1717);
            OldSubaddressNr16Bus1Id = new CrabSubaddressId(161);
            NewSubaddressNr16Bus1Id = new CrabSubaddressId(163);
            NewSubaddressNr16Bus2Id = new CrabSubaddressId(164);
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressingBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);

            _importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(_importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    _importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-2))));

            _buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    _buildingUnitWasAdded,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            _importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithHouseNumberId(HuisNr16Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-2))));

            _buildingUnit2WasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(_importSubaddress.Timestamp));
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(_importSubaddress.Timestamp));
            _buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(_importSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnit2WasAdded,
                    _buildingUnitWasReaddressed,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    _importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))));

            _buildingUnit3WasAdded = new BuildingUnitWasAdded(Gebouw1Id, NewGebouweenheidHouseNrId, NewHouseNrKey, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    _buildingUnit3WasAdded,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddress()
        {
            _importNewSubaddress1 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(NewSubaddressNr16Bus2Id)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))));

            _buildingUnit4WasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid4Id, GebouwEenheid4Key, NewAddress16Bus2Id, new BuildingUnitVersion(_importNewSubaddress1.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(AddNewHouseNumber())
                .When(_importNewSubaddress1)
                .Then(Gebouw1Id,
                    _buildingUnit4WasAdded,
                    new BuildingUnitWasNotRealized(Gebouw1Id, NewGebouweenheidHouseNrId),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, NewGebouweenheidHouseNrId),
                    new BuildingUnitAddressWasAttached(Gebouw1Id, NewAddress16Id, GebouwEenheid3Id),
                    _importNewSubaddress1.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewReaddressedSubaddress()
        {
            _importNewSubaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddress())
                .When(_importNewSubaddress2)
                .Then(Gebouw1Id,
                    _importNewSubaddress2.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldHouseNr()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            //current command
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewReaddressedSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(new Fact[] {
                    new Fact(Gebouw1Id, new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid1Id)),
                    new Fact(Gebouw1Id, new BuildingUnitAddressWasDetached(Gebouw1Id, Address16Id, GebouwEenheid1Id)),
                    new Fact(Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(Gebouw1Id)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, OldSubaddressNr16Bus1Id), _importReaddressingSubaddress.ToLegacyEvent()  }
                        })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { HuisNr16KoppelingId, HuisNr16Id },
                            { NewHuisNr16KoppelingId, NewHuisNr16Id }
                        })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                        {
                            HuisNr16KoppelingId, NewHuisNr16KoppelingId
                        })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(HuisNr16KoppelingId, HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab> {_importSubaddress.ToLegacyEvent()}},
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(NewHuisNr16KoppelingId, NewHuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>{ _importNewSubaddress1.ToLegacyEvent(), _importNewSubaddress2.ToLegacyEvent() } }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>
                            {
                                { BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId, NewSubaddressNr16Bus1Id), BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, OldSubaddressNr16Bus1Id) }
                            })
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(new AddressId(_buildingUnitWasAdded.AddressId))
                                    .WithAddressIds(new List<AddressId>()),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>{_buildingUnitWasReaddressed})
                                    .WithAddressIds(new List<AddressId>{new AddressId(_buildingUnitWasReaddressed.NewAddressId)}),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithAddressIds(new List<AddressId>{new AddressId(_buildingUnit3WasAdded.AddressId)}),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit3WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(new AddressId(_buildingUnit3WasAdded.AddressId))
                                    .WithAddressIds(new List<AddressId>()),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit4WasAdded),
                            })
                        )
                        .Build(19, EventSerializerSettings))

                });
        }

        [Fact]
        public void ReaddressSubaddressTest()
        {
            Assert(AddReaddressingOfSubaddress());
        }

        [Fact]
        public void AddHouseNumberTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddSubaddressTest()
        {
            Assert(AddSubaddress());
        }

        [Fact]
        public void AddNewHouseNrTest()
        {
            Assert(AddNewHouseNumber());
        }

        [Fact]
        public void AddNewSubaddressTest()
        {
            Assert(AddNewSubaddress());
        }

        [Fact]
        public void AddNewReaddressedSubaddressTest()
        {
            Assert(AddNewReaddressedSubaddress());
        }

        [Fact]
        public void RetireOldHouseNrTest()
        {
            Assert(RetireOldHouseNr());
        }
    }
}

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
    using WhenImportingCrabTerrainObject;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCase1AReaddressHouseNumberWithSubaddresses : SnapshotBasedTest
    {
        #region Snapshot variables
        private BuildingUnitWasAdded? _buildingUnit1WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit2WasAdded;
        private BuildingUnitWasAdded? _buildingUnit3WasAdded;
        private BuildingUnitWasReaddressed? _buildingUnit1WasReaddressed;
        private BuildingUnitWasReaddressed? _commonBuildingUnitWasReaddressed;
        private BuildingUnitWasReaddressed? _buildingUnit2WasReaddressed;
        private BuildingUnitWasReaddressed? _buildingUnit3WasReaddressed;
        private BuildingUnitWasReaddedByOtherUnitRemoval? _buildingUnitWasReaddedByOtherUnitRemoval;
        private ImportReaddressingHouseNumberFromCrab? _importReaddressingHouseNumberFromCrab;
        private ImportSubaddressFromCrab? _importSubaddressFromCrab;
        private ImportSubaddressFromCrab _importSubaddress2FromCrab;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress1FromCrab;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress2FromCrab;
        private ImportSubaddressFromCrab? _importNewSubaddress2FromCrab;
        private ImportSubaddressFromCrab? _importNewSubaddress1FromCrab;
        #endregion

        public GivenCase1AReaddressHouseNumberWithSubaddresses(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            _ = new TestCase1AData(Fixture);
        }

        protected class TestCase1AData
        {
            public TestCase1AData(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNr16KoppelingId = customizedFixture.Create<CrabTerrainObjectHouseNumberId>();
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                SubaddressNr16Bus1Id = new CrabSubaddressId(161);
                SubaddressNr16Bus2Id = new CrabSubaddressId(162);

                NewHuisNr16Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                NewSubaddressNr16Bus1Id = new CrabSubaddressId(261);
                NewSubaddressNr16Bus2Id = new CrabSubaddressId(262);
                ReaddressingBeginLocalDate = customizedFixture.Create<LocalDate>();
                ReaddressingBeginDate = new ReaddressingBeginDate(ReaddressingBeginLocalDate);
            }

            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabHouseNumberId NewHuisNr16Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }
            public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }
            public CrabSubaddressId NewSubaddressNr16Bus2Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid5Key => GebouwEenheid1Key;

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid2IdV2 => BuildingUnitId.Create(GebouwEenheid2Key, 2);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid3IdV2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid4IdV2 => BuildingUnitId.Create(GebouwEenheid4Key, 2);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 2);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address16Bus1Id => AddressId.CreateFor(SubaddressNr16Bus1Id);
            public AddressId Address16Bus2Id => AddressId.CreateFor(SubaddressNr16Bus2Id);
            public AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
            public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);
            public AddressId NewAddress16Bus2Id => AddressId.CreateFor(NewSubaddressNr16Bus2Id);
            public ReaddressingBeginDate ReaddressingBeginDate { get; }
            public LocalDate ReaddressingBeginLocalDate { get; set; }
        }
        protected TestCase1AData _ { get; }

        public IEventCentricTestSpecificationBuilder T1()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            _buildingUnit1WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit1WasAdded,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2()
        {
            _importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _buildingUnit2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(_importSubaddressFromCrab.Timestamp));
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(_importSubaddressFromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T1())
                .When(_importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit2WasAdded,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    _importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3()
        {
            _importSubaddress2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _buildingUnit3WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(_importSubaddress2FromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(_importSubaddress2FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit3WasAdded,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    _importSubaddress2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3ReaddressHouseNr()
        {
            _importReaddressingHouseNumberFromCrab = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(_.NewHuisNr16Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            _buildingUnit1WasReaddressed = new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate);
            _commonBuildingUnitWasReaddressed = new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid3Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(_importReaddressingHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit1WasReaddressed,
                    _commonBuildingUnitWasReaddressed,
                    _importReaddressingHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3ReaddressSubaddress1()
        {
            _importReaddressingSubaddress1FromCrab = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldSubaddressId(_.SubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            _buildingUnit2WasReaddressed = new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2Id, _.Address16Bus1Id, _.NewAddress16Bus1Id, _.ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(T3ReaddressHouseNr())
                .When(_importReaddressingSubaddress1FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit2WasReaddressed,
                    _importReaddressingSubaddress1FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3ReaddressSubaddress2()
        {
            _importReaddressingSubaddress2FromCrab = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldSubaddressId(_.SubaddressNr16Bus2Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewSubaddressId(_.NewSubaddressNr16Bus2Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            _buildingUnit3WasReaddressed = new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid4Id, _.Address16Bus2Id, _.NewAddress16Bus2Id, _.ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(T3ReaddressSubaddress1())
                .When(_importReaddressingSubaddress2FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit3WasReaddressed,
                    _importReaddressingSubaddress2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            _importNewSubaddress2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-1)))); ;

            _buildingUnitWasReaddedByOtherUnitRemoval = new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.NewAddress16Id, new BuildingUnitVersion(_importNewSubaddress2FromCrab.Timestamp), _.GebouwEenheid1Id);
            return new AutoFixtureScenario(Fixture)
                .Given(T3ReaddressSubaddress2())
                .When(_importNewSubaddress2FromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Bus2Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id),
                    _buildingUnitWasReaddedByOtherUnitRemoval,
                    _importNewSubaddress2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T5()
        {
            _importNewSubaddress1FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-1)))); ;

            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(_importNewSubaddress1FromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    _importNewSubaddress1FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T5())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid5Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T7()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectFromCrab = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T6())
                .When(importTerrainObjectFromCrab)
                .Then(new Fact[]
                {
                    new Fact(_.Gebouw1Id, new BuildingWasNotRealized(_.Gebouw1Id, new BuildingUnitId[0], new BuildingUnitId[0])),
                    new Fact(_.Gebouw1Id, importTerrainObjectFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.NewHuisNr16KoppelingId, _.NewHuisNr16Id }
                        })
                        .WithStatus(BuildingStatus.NotRealized)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId, _.HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab> { _importSubaddressFromCrab.ToLegacyEvent(), _importSubaddress2FromCrab.ToLegacyEvent(), _importNewSubaddress2FromCrab.ToLegacyEvent(),_importNewSubaddress1FromCrab.ToLegacyEvent() } },
                        })
                        .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>
                        {
                            { _.GebouwEenheid1Key, _importReaddressingHouseNumberFromCrab.ToLegacyEvent() }
                        })
                        .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                        {
                            { _.GebouwEenheid2Key, _importReaddressingSubaddress1FromCrab.ToLegacyEvent() },
                            { _.GebouwEenheid4Key, _importReaddressingSubaddress2FromCrab.ToLegacyEvent() }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit1WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.NewAddress16Id)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithReaddressedEvents(_buildingUnit1WasReaddressed),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.NewAddress16Bus1Id)
                                    .WithRetiredBySelf()
                                    .WithReaddressedEvents(_buildingUnit2WasReaddressed),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Retired)
                                    .WithPreviousAddressId(_.NewAddress16Id)
                                    .WithReaddressedEvents(_commonBuildingUnitWasReaddressed),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit3WasAdded)
                                    .WithRetiredBySelf()
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.NewAddress16Bus2Id)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithReaddressedEvents(_buildingUnit3WasReaddressed),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasReaddedByOtherUnitRemoval)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.NewAddress16Id)
                            })
                            .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>
                            {
                                { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId), _.GebouwEenheid1Key },
                                { BuildingUnitKey.Create(_importReaddressingSubaddress1FromCrab.TerrainObjectId, _importReaddressingSubaddress1FromCrab.NewTerrainObjectHouseNumberId, _importReaddressingSubaddress1FromCrab.NewSubaddressId), _.GebouwEenheid2Key },
                                { BuildingUnitKey.Create(_importReaddressingSubaddress2FromCrab.TerrainObjectId, _importReaddressingSubaddress2FromCrab.NewTerrainObjectHouseNumberId, _importReaddressingSubaddress2FromCrab.NewSubaddressId), _.GebouwEenheid4Key }
                            }))
                        .Build(32, EventSerializerSettings))
                });
        }

        [Fact]
        public void TestT1()
        {
            Assert(T1());
        }

        [Fact]
        public void TestT2()
        {
            Assert(T2());
        }

        [Fact]
        public void TestT3()
        {
            Assert(T3());
        }

        [Fact]
        public void TestT3ReaddressHouseNr()
        {
            Assert(T3ReaddressHouseNr());
        }

        [Fact]
        public void TestT3ReaddressSubaddress1()
        {
            Assert(T3ReaddressSubaddress1());
        }

        [Fact]
        public void TestT3ReaddressSubaddress2()
        {
            Assert(T3ReaddressSubaddress2());
        }

        [Fact]
        public void TestT4()
        {
            Assert(T4());
        }

        [Fact]
        public void TestT5()
        {
            Assert(T5());
        }

        [Fact]
        public void TestT6()
        {
            Assert(T6());
        }

        [Fact]
        public void TestT7()
        {
            Assert(T7());
        }
    }
}

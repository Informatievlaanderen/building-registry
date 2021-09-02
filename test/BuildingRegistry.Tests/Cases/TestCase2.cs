namespace BuildingRegistry.Tests.Cases
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
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

    /// <summary>
    ///     Test scenario Case 2 can be found on
    ///     https://vlaamseoverheid.sharepoint.com/sites/aiv/tfs/gebouwenregister/Shared%20Documents/Projectdata/Gebouwenregister/TO%20BE%20Analyse/CRAB%20Initialisatie-Synchronisatie/Initialisatie_gebouweenheden.vsdx
    /// </summary>
    public class TestCase2 : SnapshotBasedTest
    {
        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumber16FromCrab;
        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumber18FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress18_1FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress18_2FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress16_1FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress16_2FromCrab;
        private BuildingUnitWasAdded _buildingUnit16WasAdded;
        private BuildingUnitWasAdded? _buildingUnit18WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit18_1WasAdded;
        private BuildingUnitWasAdded? _buildingUnit18_2WasAdded;
        private BuildingUnitWasAdded? _buildingUnit16_1WasAdded;
        private BuildingUnitWasAdded? _buildingUnit16_2WasAdded;

        public TestCase2(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCase2Data(Fixture);
        }

        protected class TestCase2Data
        {
            public TestCase2Data(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNr16KoppelingId = customizedFixture.Create<CrabTerrainObjectHouseNumberId>();
                HuisNr18KoppelingId = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                HuisNr18Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                SubaddressNr18Bus1Id = new CrabSubaddressId(customizedFixture.Create<int>());
                SubaddressNr18Bus2Id = new CrabSubaddressId(customizedFixture.Create<int>());
                SubaddressNr16Bus1Id = new CrabSubaddressId(customizedFixture.Create<int>());
                SubaddressNr16Bus2Id = new CrabSubaddressId(customizedFixture.Create<int>());
            }


            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr18KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabHouseNumberId HuisNr18Id { get; }
            public CrabSubaddressId SubaddressNr18Bus1Id { get; }
            public CrabSubaddressId SubaddressNr18Bus2Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr18Bus1Id);

            public BuildingUnitKey GebouwEenheid5Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr18Bus2Id);

            public BuildingUnitKey GebouwEenheid6Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid7Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 1);
            public BuildingUnitId GebouwEenheid6Id => BuildingUnitId.Create(GebouwEenheid6Key, 1);
            public BuildingUnitId GebouwEenheid7Id => BuildingUnitId.Create(GebouwEenheid7Key, 1);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address18Id => AddressId.CreateFor(HuisNr18Id);
            public AddressId Address18Bus1Id => AddressId.CreateFor(SubaddressNr18Bus1Id);
            public AddressId Address18Bus2Id => AddressId.CreateFor(SubaddressNr18Bus2Id);
        }

        protected TestCase2Data _ { get; }

        public IEventCentricTestSpecificationBuilder T1()
        {
            //koppel huisnr 16
            _importTerrainObjectHouseNumber16FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id);

            _buildingUnit16WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(_importTerrainObjectHouseNumber16FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(_importTerrainObjectHouseNumber16FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit16WasAdded,
                    _importTerrainObjectHouseNumber16FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2()
        {
            //koppel huisnr 18
            _importTerrainObjectHouseNumber18FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id);

            _buildingUnit18WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address18Id, new BuildingUnitVersion(_importTerrainObjectHouseNumber18FromCrab.Timestamp));
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(_importTerrainObjectHouseNumber18FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T1())
                .When(_importTerrainObjectHouseNumber18FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit18WasAdded,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    _importTerrainObjectHouseNumber18FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3()
        {
            _importSubaddress18_1FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr18Bus1Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);

            _buildingUnit18_1WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key,
                AddressId.CreateFor(_.SubaddressNr18Bus1Id), new BuildingUnitVersion(_importSubaddress18_1FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(_importSubaddress18_1FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit18_1WasAdded,
                    _importSubaddress18_1FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            _importSubaddress18_2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr18Bus2Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);

            _buildingUnit18_2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, AddressId.CreateFor(_.SubaddressNr18Bus2Id), new BuildingUnitVersion(_importSubaddress18_2FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(_importSubaddress18_2FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit18_2WasAdded,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id,_.Address18Id,  _.GebouwEenheid2Id),
                    //new AddressWasDetached(_.Gebouw1Id,_.Address18Id,  _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address18Id, _.GebouwEenheid3Id),
                    _importSubaddress18_2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T5()
        {
            _importSubaddress16_1FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _buildingUnit16_1WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, AddressId.CreateFor(_.SubaddressNr16Bus1Id), new BuildingUnitVersion(_importSubaddress16_1FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(_importSubaddress16_1FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit16_1WasAdded,
                    _importSubaddress16_1FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6()
        {
            _importSubaddress16_2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _buildingUnit16_2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid7Id, _.GebouwEenheid7Key, AddressId.CreateFor(_.SubaddressNr16Bus2Id), new BuildingUnitVersion(_importSubaddress16_2FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T5())
                .When(_importSubaddress16_2FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit16_2WasAdded,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    _importSubaddress16_2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6_WithSnapshot()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            _importSubaddress16_2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _buildingUnit16_2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid7Id, _.GebouwEenheid7Key, AddressId.CreateFor(_.SubaddressNr16Bus2Id), new BuildingUnitVersion(_importSubaddress16_2FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(T5())
                .When(_importSubaddress16_2FromCrab)
                .Then(
                    new Fact(_.Gebouw1Id, _buildingUnit16_2WasAdded),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id)),
                    new Fact(_.Gebouw1Id, _importSubaddress16_2FromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.HuisNr16KoppelingId, _.HuisNr16Id },
                            { _.HuisNr18KoppelingId, _.HuisNr18Id }
                        })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId> { _.HuisNr16KoppelingId, _.HuisNr18KoppelingId })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr18KoppelingId, _.HuisNr18Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress18_1FromCrab.ToLegacyEvent(), _importSubaddress18_2FromCrab.ToLegacyEvent()} },
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId, _.HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress16_1FromCrab.ToLegacyEvent(), _importSubaddress16_2FromCrab.ToLegacyEvent()} },
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.Address16Id),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit18WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address18Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithAddressIds(new List<AddressId>{_.Address18Id, _.Address16Id}),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit18_1WasAdded),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit18_2WasAdded),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16_1WasAdded),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16_2WasAdded),
                            }))
                        .Build(20, EventSerializerSettings))
                );
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4RetireHouseNumberWithSubaddresses()
        {
            var importRetireTerrainObjectHouseNumber18FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id);

            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(importRetireTerrainObjectHouseNumber18FromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Bus1Id, _.GebouwEenheid4Id),
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Bus2Id, _.GebouwEenheid5Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Id, _.GebouwEenheid3Id),
                    importRetireTerrainObjectHouseNumber18FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT6RetireHouseNumberWithSubaddresses()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id);
            return new AutoFixtureScenario(Fixture)
                .Given(T6())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(new Fact(_.Gebouw1Id, new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid2Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Bus1Id, _.GebouwEenheid4Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid2Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Bus2Id, _.GebouwEenheid5Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Id, _.GebouwEenheid3Id)),
                    new Fact(_.Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.HuisNr16KoppelingId, _.HuisNr16Id },
                            { _.HuisNr18KoppelingId, _.HuisNr18Id }
                        })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId> {_.HuisNr16KoppelingId, _.HuisNr18KoppelingId })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr18KoppelingId, _.HuisNr18Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress18_1FromCrab.ToLegacyEvent(), _importSubaddress18_2FromCrab.ToLegacyEvent()} },
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId, _.HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress16_1FromCrab.ToLegacyEvent(), _importSubaddress16_2FromCrab.ToLegacyEvent()} },
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.Address16Id),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit18WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address18Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithAddressIds(new List<AddressId>{_.Address16Id})
                                    .WithPreviousAddressId(_.Address18Id),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit18_1WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.Address18Bus1Id)
                                    .WithRetiredByParent(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit18_2WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.Address18Bus2Id)
                                    .WithRetiredByParent(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16_1WasAdded),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16_2WasAdded),
                            }))
                        .Build(26, EventSerializerSettings))
                );
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
        public void TestT6_WithSnapshot()
        {
            Assert(T6_WithSnapshot());
        }

        [Fact]
        public void TestBasedOnT4RetireHouseNumberWithSubaddresses()
        {
            Assert(BasedOnT4RetireHouseNumberWithSubaddresses());
        }

        [Fact]
        public void TestBasedOnT6RetireHouseNumberWithSubaddresses()
        {
            Assert(BasedOnT6RetireHouseNumberWithSubaddresses());
        }
    }
}

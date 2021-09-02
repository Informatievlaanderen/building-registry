namespace BuildingRegistry.Tests.Cases
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

    /// <summary>
    /// terreinobject id: 7050782
    ///select * from odb.tblTerreinObject_huisNummer where terreinObjectId =7050782
    ///select * from cdb.tblterreinobject_huisnummer_hist where terreinObjectId = 7050782
    ///
    ///select * from odb.tblSubAdres where huisnummerid in (3258112, 2102736)
    ///select * from cdb.tblSubAdres_hist where huisnummerid in (3258112, 2102736)
    /// </summary>
    public class DeleteSubaddressesLinkedToNewHouseNumberAfterRemoved : SnapshotBasedTest
    {
        #region Snapshot variables

        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumber15FromCrab;
        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumber16FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress17_1FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress17_2FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress17_3FromCrab;
        private ImportSubaddressFromCrab? _importSubaddress17_4FromCrab;
        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumber17FromCrab;
        private ImportTerrainObjectHouseNumberFromCrab? _importNewTerrainObjectHouseNumber15FromCrab;
        private BuildingUnitWasAdded? _buildingUnit15WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit16WasAdded;
        private BuildingUnitWasAdded? _buildingUnit17WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAddedV2;
        private BuildingUnitWasAdded? _buildingUnit17_1WasAdded;
        private BuildingUnitWasAdded? _buildingUnit17_2WasAdded;
        private BuildingUnitWasAdded? _buildingUnit17_3WasAdded;
        private BuildingUnitWasAdded? _buildingUnit17_4WasAdded;
        private BuildingUnitWasAdded? _buildingUnitNew15WasAdded;

        #endregion
        
        public DeleteSubaddressesLinkedToNewHouseNumberAfterRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture 
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCaseData(Fixture);
        }

        protected class TestCaseData
        {
            public TestCaseData(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNrKoppeling1Id = customizedFixture.Create<CrabTerrainObjectHouseNumberId>();
                HuisNrKoppeling2Id = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                HuisNrKoppeling3Id = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                HuisNr15Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                HuisNr17Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                SubaddressNr16Bus1Id = new CrabSubaddressId(161);
                SubaddressNr16Bus2Id = new CrabSubaddressId(162);
                SubaddressNr16Bus3Id = new CrabSubaddressId(163);
                SubaddressNr16Bus4Id = new CrabSubaddressId(164);
            }


            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNrKoppeling1Id { get; } //3707305
            public CrabTerrainObjectHouseNumberId HuisNrKoppeling2Id { get; } //3705627
            public CrabTerrainObjectHouseNumberId HuisNrKoppeling3Id { get; } //8127734
            public CrabHouseNumberId HuisNr15Id { get; } //1229249
            public CrabHouseNumberId HuisNr16Id { get; } //2102736
            public CrabHouseNumberId HuisNr17Id { get; } //3258112
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }
            public CrabSubaddressId SubaddressNr16Bus3Id { get; }
            public CrabSubaddressId SubaddressNr16Bus4Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling2Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid5Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid6Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus3Id);

            public BuildingUnitKey GebouwEenheid7Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus4Id);

            public BuildingUnitKey GebouwEenheid8Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling3Id);

            //public BuildingUnitKey GebouwEenheid9Key =>
            //    BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr16Bus4Id);

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid3IdV2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 1);
            public BuildingUnitId GebouwEenheid5IdV2 => BuildingUnitId.Create(GebouwEenheid5Key, 2);
            public BuildingUnitId GebouwEenheid6Id => BuildingUnitId.Create(GebouwEenheid6Key, 1);
            public BuildingUnitId GebouwEenheid7Id => BuildingUnitId.Create(GebouwEenheid7Key, 1);
            public BuildingUnitId GebouwEenheid8Id => BuildingUnitId.Create(GebouwEenheid8Key, 1);
            //public BuildingUnitId GebouwEenheid9Id => BuildingUnitId.Create(GebouwEenheid9Key, 1);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address15Id => AddressId.CreateFor(HuisNr15Id);
            public AddressId Address17Id => AddressId.CreateFor(HuisNr17Id);
        }
        protected TestCaseData _ { get; }


        public IEventCentricTestSpecificationBuilder AddHouseNumber15()
        {
            _importTerrainObjectHouseNumber15FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)))
                .WithHouseNumberId(_.HuisNr15Id);

            _buildingUnit15WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address15Id, new BuildingUnitVersion(_importTerrainObjectHouseNumber15FromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(_importTerrainObjectHouseNumber15FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit15WasAdded,
                    _importTerrainObjectHouseNumber15FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumber16()
        {
            _importTerrainObjectHouseNumber16FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling2Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)))
                .WithHouseNumberId(_.HuisNr16Id);

            _buildingUnit16WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Id, new BuildingUnitVersion(_importTerrainObjectHouseNumber16FromCrab.Timestamp));
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(_importTerrainObjectHouseNumber16FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumber15())
                .When(_importTerrainObjectHouseNumber16FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit16WasAdded,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    _importTerrainObjectHouseNumber16FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress1WithFutureHouseNumber()
        {
            _importSubaddress17_1FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumber16())
                .When(_importSubaddress17_1FromCrab)
                .Then(_.Gebouw1Id, _importSubaddress17_1FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress2WithFutureHouseNumber()
        {
            _importSubaddress17_2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress1WithFutureHouseNumber())
                .When(_importSubaddress17_2FromCrab)
                .Then(_.Gebouw1Id, _importSubaddress17_2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress3WithFutureHouseNumber()
        {
            _importSubaddress17_3FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus3Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress2WithFutureHouseNumber())
                .When(_importSubaddress17_3FromCrab)
                .Then(_.Gebouw1Id, _importSubaddress17_3FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress4WithFutureHouseNumber()
        {
            _importSubaddress17_4FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus4Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress3WithFutureHouseNumber())
                .When(_importSubaddress17_4FromCrab)
                .Then(_.Gebouw1Id, _importSubaddress17_4FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSwitchHouseNumber15ToHouseNumber17()
        {
            _importTerrainObjectHouseNumber17FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(2))))
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithHouseNumberId(_.HuisNr17Id);

            _buildingUnit17WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address17Id, new BuildingUnitVersion(_importTerrainObjectHouseNumber17FromCrab.Timestamp));
            _commonBuildingUnitWasAddedV2 = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(_importTerrainObjectHouseNumber17FromCrab.Timestamp));

            _buildingUnit17_1WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, AddressId.CreateFor(_.SubaddressNr16Bus1Id), new BuildingUnitVersion(_importTerrainObjectHouseNumber17FromCrab.Timestamp));
            _buildingUnit17_2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, AddressId.CreateFor(_.SubaddressNr16Bus2Id), new BuildingUnitVersion(_importTerrainObjectHouseNumber17FromCrab.Timestamp));
            _buildingUnit17_3WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, AddressId.CreateFor(_.SubaddressNr16Bus3Id), new BuildingUnitVersion(_importTerrainObjectHouseNumber17FromCrab.Timestamp));
            _buildingUnit17_4WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid7Id, _.GebouwEenheid7Key, AddressId.CreateFor(_.SubaddressNr16Bus4Id), new BuildingUnitVersion(_importTerrainObjectHouseNumber17FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress4WithFutureHouseNumber())
                .When(_importTerrainObjectHouseNumber17FromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),
                    _buildingUnit17WasAdded,
                    _commonBuildingUnitWasAddedV2,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    _buildingUnit17_1WasAdded,
                    _buildingUnit17_2WasAdded,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address17Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address17Id, _.GebouwEenheid3IdV2),
                    _buildingUnit17_3WasAdded,
                    _buildingUnit17_4WasAdded,
                    _importTerrainObjectHouseNumber17FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumber15WithNewRelationRetired()
        {
            _importNewTerrainObjectHouseNumber15FromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling3Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(3))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithHouseNumberId(_.HuisNr15Id);

            _buildingUnitNew15WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid8Id, _.GebouwEenheid8Key, _.Address15Id, new BuildingUnitVersion(_importNewTerrainObjectHouseNumber15FromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(AddSwitchHouseNumber15ToHouseNumber17())
                .When(_importNewTerrainObjectHouseNumber15FromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnitNew15WasAdded,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid8Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address15Id, _.GebouwEenheid8Id),
                    _importNewTerrainObjectHouseNumber15FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder DeleteRelationWithHouseNr17()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(4))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithModification(CrabModification.Delete);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumber15WithNewRelationRetired())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid4Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid5Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid6Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address17Id, _.GebouwEenheid3IdV2)),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid7Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3IdV2)),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1IdV2)),
                    new Fact(_.Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.HuisNrKoppeling2Id, _.HuisNr16Id },
                            { _.HuisNrKoppeling3Id, _.HuisNr15Id }
                        })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId> {_.HuisNrKoppeling1Id, _.HuisNrKoppeling2Id, _.HuisNrKoppeling3Id})
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNrKoppeling1Id, _.HuisNr17Id), new List<AddressSubaddressWasImportedFromCrab> { _importSubaddress17_1FromCrab.ToLegacyEvent(), _importSubaddress17_2FromCrab.ToLegacyEvent(), _importSubaddress17_3FromCrab.ToLegacyEvent(), _importSubaddress17_4FromCrab.ToLegacyEvent()} }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit15WasAdded)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit16WasAdded),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit17WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(_.Address17Id)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAddedV2)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithPreviousAddressId(_.Address17Id)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit17_1WasAdded)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit17_2WasAdded)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit17_3WasAdded)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit17_4WasAdded)
                                    .WithRemoved(),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitNew15WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address15Id)
                                    .WithAddressIds(new List<AddressId>())
                            }))
                        .Build(35, EventSerializerSettings)
                    ));

        }

        [Fact]
        public void AddHouseNumber15Test()
        {
            Assert(AddHouseNumber15());
        }

        [Fact]
        public void AddHouseNumber16Test()
        {
            Assert(AddHouseNumber16());
        }

        [Fact]
        public void AddSubaddress1Test()
        {
            Assert(AddSubaddress1WithFutureHouseNumber());
        }

        [Fact]
        public void AddSubaddress2Test()
        {
            Assert(AddSubaddress2WithFutureHouseNumber());
        }

        [Fact]
        public void AddSubaddress3Test()
        {
            Assert(AddSubaddress3WithFutureHouseNumber());
        }

        [Fact]
        public void AddSubaddress4Test()
        {
            Assert(AddSubaddress4WithFutureHouseNumber());
        }

        [Fact]
        public void SwitchHouseNr15To17Test()
        {
            Assert(AddSwitchHouseNumber15ToHouseNumber17());
        }

        [Fact]
        public void AddHouseNr15WithNewRelationRetiredTest()
        {
            Assert(AddHouseNumber15WithNewRelationRetired());
        }

        [Fact]
        public void DeleteRelationWithHouseNr17Test()
        {
            Assert(DeleteRelationWithHouseNr17());
        }
    }

}

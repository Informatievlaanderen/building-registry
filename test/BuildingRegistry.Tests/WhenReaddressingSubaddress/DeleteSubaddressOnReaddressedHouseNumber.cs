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
    using WhenReaddressingHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Delete readdressed subaddress on readdressed housenumber with active new subaddress
    /// </summary>
    public class DeleteSubaddressOnReaddressedHouseNumber : SnapshotBasedTest
    {
        #region Snapshot variables

        private ImportReaddressingHouseNumberFromCrab? _importReaddressingHouseNumberFromCrab;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddressFromCrab;
        private ImportSubaddressFromCrab? _importNewSubaddress2;
        private ImportSubaddressFromCrab? _importNewSubaddress;
        private ImportTerrainObjectHouseNumberFromCrab? _importNewTerrainObjectHouseNumberFromCrab;
        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumberFromCrab;
        private ImportSubaddressFromCrab? _importOldSubaddress;
        private ImportTerrainObjectHouseNumberFromCrab? _retireOldimportTerrainObjectHouseNumberFromCrab;
        private ImportSubaddressFromCrab? _retireOldImportSubaddress;
        private BuildingUnitWasAdded? _buildingUnit1WasAdded;
        private BuildingUnitWasReaddressed? _buildingUnit1WasReaddressed;
        private BuildingUnitWasAdded? _buildingUnit2WasAdded;
        private BuildingUnitWasReaddressed? _buildingUnit2WasReaddressed;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnit3WasAdded;

        #endregion

        public DeleteSubaddressOnReaddressedHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId, NewSubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid5Key => BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);

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

        public IEventCentricTestSpecificationBuilder ReaddressHouseNumber()
        {
            _importReaddressingHouseNumberFromCrab = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(_.NewHuisNr16Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(_importReaddressingHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    _importReaddressingHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ReaddressSubaddress()
        {
            _importReaddressingSubaddressFromCrab = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldSubaddressId(_.SubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddressHouseNumber())
                .When(_importReaddressingSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    _importReaddressingSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportOldSubaddress()
        {
            _importOldSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddressSubaddress())
                .When(_importOldSubaddress)
                .Then(_.Gebouw1Id,
                    _importOldSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberWithImportedSubaddressUnits()
        {
            _importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-2))));

            _buildingUnit1WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));
            _buildingUnit1WasReaddressed = new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate);
            _buildingUnit2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));
            _buildingUnit2WasReaddressed = new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2Id, _.Address16Bus1Id, _.NewAddress16Bus1Id, _.ReaddressingBeginDate);
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given(ImportOldSubaddress())
                .When(_importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    _buildingUnit1WasAdded,
                    _buildingUnit1WasReaddressed,
                    _buildingUnit2WasAdded,
                    _buildingUnit2WasReaddressed,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewHouseNumber()
        {
            _importNewTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberWithImportedSubaddressUnits())
                .When(_importNewTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    _importNewTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddress()
        {
            _importNewSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewHouseNumber())
                .When(_importNewSubaddress)
                .Then(_.Gebouw1Id,
                    _importNewSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldHouseNumber()
        {
            _retireOldimportTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddHours(5))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddress())
                .When(_retireOldimportTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    _retireOldimportTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldSubaddress()
        {
            _retireOldImportSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddHours(6))));

            return new AutoFixtureScenario(Fixture)
                .Given(RetireOldHouseNumber())
                .When(_retireOldImportSubaddress)
                .Then(_.Gebouw1Id,
                    _retireOldImportSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddress2()
        {
            _importNewSubaddress2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.NewSubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(1))));

            _buildingUnit3WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.NewAddress16Bus2Id, new BuildingUnitVersion(_importNewSubaddress2.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(RetireOldSubaddress())
                .When(_importNewSubaddress2)
                .Then(_.Gebouw1Id,
                    _buildingUnit3WasAdded,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id),
                    _importNewSubaddress2.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RemoveReaddressedSubaddress()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithModification(CrabModification.Delete)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(1))));

            var buildingUnitWasReaddedByOtherUnitRemoval = new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.NewAddress16Id, new BuildingUnitVersion(importSubaddress.Timestamp), _.GebouwEenheid1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddress2())
                .When(importSubaddress)
                .Then(new Fact[]
                {
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id)),
                    new Fact(_.Gebouw1Id, buildingUnitWasReaddedByOtherUnitRemoval),
                    new Fact(_.Gebouw1Id, importSubaddress.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.NewHuisNr16KoppelingId, _.NewHuisNr16Id },
                            { _.HuisNr16KoppelingId, _.HuisNr16Id }
                        })
                        .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(_.Gebouw1CrabTerrainObjectId, _.HuisNr16KoppelingId), _importReaddressingHouseNumberFromCrab.ToLegacyEvent() }
                        })
                        .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(_.Gebouw1CrabTerrainObjectId, _.HuisNr16KoppelingId, _importReaddressingSubaddressFromCrab.OldSubaddressId), _importReaddressingSubaddressFromCrab.ToLegacyEvent() }
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId})
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId, _.HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab> { _importOldSubaddress.ToLegacyEvent(), _retireOldImportSubaddress.ToLegacyEvent() } },
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.NewHuisNr16KoppelingId, _.NewHuisNr16Id), new List<AddressSubaddressWasImportedFromCrab> { _importNewSubaddress.ToLegacyEvent(), _importNewSubaddress2.ToLegacyEvent(), importSubaddress.ToLegacyEvent() } }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit1WasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.NewAddress16Id)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>
                                    {
                                        _buildingUnit1WasReaddressed
                                    }),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                    .WithAddressIds(new List<AddressId>{_.NewAddress16Bus1Id})
                                    .WithRemoved()
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>
                                    {
                                        _buildingUnit2WasReaddressed
                                    }),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithPreviousAddressId(_.NewAddress16Id),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit3WasAdded),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasReaddedByOtherUnitRemoval)
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>{_buildingUnit1WasReaddressed})
                            })
                            .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>
                            {
                                { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId), BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId)  },
                                { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId, _importReaddressingSubaddressFromCrab.NewSubaddressId), BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId, _importReaddressingSubaddressFromCrab.OldSubaddressId) }
                            })
                        )
                        .Build(23, EventSerializerSettings))
                });
        }

        [Fact]
        public void ReaddressHouseNumberTest()
        {
            Assert(ReaddressHouseNumber());
        }

        [Fact]
        public void ReaddressSubaddressTest()
        {
            Assert(ReaddressSubaddress());
        }

        [Fact]
        public void ImportOldSubaddressTest()
        {
            Assert(ImportOldSubaddress());
        }

        [Fact]
        public void AddHouseNrWithSubaddressTest()
        {
            Assert(AddHouseNumberWithImportedSubaddressUnits());
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
        public void RetireOldHouseNumberTest()
        {
            Assert(RetireOldHouseNumber());
        }

        [Fact]
        public void RetireOldSubaddressTest()
        {
            Assert(RetireOldSubaddress());
        }

        [Fact]
        public void AddNewSubaddress2Test()
        {
            Assert(AddNewSubaddress2());
        }

        [Fact]
        public void RemoveReaddressedSubaddressTest()
        {
            Assert(RemoveReaddressedSubaddress());
        }
    }
}

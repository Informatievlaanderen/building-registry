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

    public class CaseRetireAfterReaddressed : SnapshotBasedTest
    {
        private LocalDate _readdressingBeginDate;
        private ImportReaddressingHouseNumberFromCrab? _importReaddressingHouseNumberFromCrab;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress;
        private ImportTerrainObjectHouseNumberFromCrab? _importTerrainObjectHouseNumberFromCrab;
        private ImportSubaddressFromCrab? _importNewSubaddress;
        private ImportSubaddressFromCrab? _importNewReaddressedSubaddress;
        private ImportSubaddressFromCrab? _importOldSubaddress;
        private BuildingUnitWasAdded? _buildingUnitWasAdded;
        private BuildingUnitWasReaddressed? _buildingUnitWasReaddressed;
        private BuildingUnitWasAdded? _buildingUnitSubaddressWasAdded;
        private BuildingUnitWasReaddressed? _buildingUnitSubaddressWasReaddressed;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasAdded? _buildingUnitNewSubaddressWasAdded;

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
        private BuildingUnitKey NewGebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);
        public BuildingUnitKey OldGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, OldSubaddressNr16Bus1Id);
        public BuildingUnitKey NewGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, NewSubaddressNr16Bus1Id);
        public BuildingUnitKey GebouwEenheid3Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel
        public BuildingUnitKey GebouwEenheid4Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId, NewSubaddressNr16Bus2Id);

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId NewGebouwEenheid1Id => BuildingUnitId.Create(NewGebouwEenheid1Key, 2);
        private BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(OldGebouwEenheid2Key, 1);
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
        public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);

        private AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);

        public AddressId OldAddress16Bus1Id => AddressId.CreateFor(OldSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus2Id => AddressId.CreateFor(NewSubaddressNr16Bus2Id);

        public CaseRetireAfterReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            HuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
            HuisNr16Id = new CrabHouseNumberId(1666);
            NewHuisNr16Id = new CrabHouseNumberId(1777);
            OldSubaddressNr16Bus1Id = new CrabSubaddressId(161);
            NewSubaddressNr16Bus1Id = new CrabSubaddressId(163);
            NewSubaddressNr16Bus2Id = new CrabSubaddressId(164);
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressingBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNr()
        {
            _importReaddressingHouseNumberFromCrab = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldHouseNumberId(HuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(_importReaddressingHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    _importReaddressingHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            _importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNr())
                .When(_importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    _importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            _importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id);

            _buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(_importTerrainObjectHouseNumberFromCrab.Timestamp));
            _buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, Address16Id, NewAddress16Id, ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(_importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    _buildingUnitWasAdded,
                    _buildingUnitWasReaddressed,
                    _importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            _importOldSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithHouseNumberId(HuisNr16Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))));

            _buildingUnitSubaddressWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(_importOldSubaddress.Timestamp));
            _buildingUnitSubaddressWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate);
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(_importOldSubaddress.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(_importOldSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnitSubaddressWasAdded,
                    _buildingUnitSubaddressWasReaddressed,
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    _importOldSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedSubaddressWithNewSubaddressId()
        {
            _importNewReaddressedSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(_importNewReaddressedSubaddress)
                .Then(Gebouw1Id,
                    _importNewReaddressedSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddressWithNewSubaddressId()
        {
            _importNewSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus2Id)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(2))));

            _buildingUnitNewSubaddressWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid4Id, GebouwEenheid4Key, NewAddress16Bus2Id, new BuildingUnitVersion(_importNewSubaddress.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressedSubaddressWithNewSubaddressId())
                .When(_importNewSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnitNewSubaddressWasAdded,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(Gebouw1Id, NewAddress16Id, GebouwEenheid3Id),
                    _importNewSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldHouseNr()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(3))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddressWithNewSubaddressId())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldSubaddress()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id)
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(3))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddressWithNewSubaddressId())
                .When(importSubaddressFromCrab)
                .Then(new Fact[]
                {
                    new Fact(Gebouw1Id, importSubaddressFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(Gebouw1Id)
                        .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(_importReaddressingSubaddress.TerrainObjectId, _importReaddressingSubaddress.OldTerrainObjectHouseNumberId, _importReaddressingSubaddress.OldSubaddressId), _importReaddressingSubaddress.ToLegacyEvent() }
                        })
                        .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId), _importReaddressingHouseNumberFromCrab.ToLegacyEvent() }
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId, _importReaddressingHouseNumberFromCrab.NewHouseNumberId },
                            { _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId, _importReaddressingHouseNumberFromCrab.OldHouseNumberId }
                        })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            {
                                new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_importOldSubaddress.TerrainObjectHouseNumberId, _importOldSubaddress.HouseNumberId),
                                new List<AddressSubaddressWasImportedFromCrab> {_importOldSubaddress.ToLegacyEvent(), importSubaddressFromCrab.ToLegacyEvent() }
                            },
                            {
                                new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_importNewSubaddress.TerrainObjectHouseNumberId, _importNewSubaddress.HouseNumberId),
                                new List<AddressSubaddressWasImportedFromCrab> { _importNewReaddressedSubaddress.ToLegacyEvent(), _importNewSubaddress.ToLegacyEvent() }
                            }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(AddressId.CreateFor(_importReaddressingHouseNumberFromCrab.NewHouseNumberId))
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>{ _buildingUnitWasReaddressed }),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitSubaddressWasAdded)
                                    .WithAddressIds(new List<AddressId>{AddressId.CreateFor(_importReaddressingSubaddress.NewSubaddressId)})
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed> { _buildingUnitSubaddressWasReaddressed }),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithAddressIds(new List<AddressId>{AddressId.CreateFor(_importReaddressingHouseNumberFromCrab.NewHouseNumberId)}),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitNewSubaddressWasAdded)
                            })
                            .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>
                            {
                                { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId), BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId) },
                                { BuildingUnitKey.Create(_importReaddressingSubaddress.TerrainObjectId, _importReaddressingSubaddress.NewTerrainObjectHouseNumberId, _importReaddressingSubaddress.NewSubaddressId), BuildingUnitKey.Create(_importReaddressingSubaddress.TerrainObjectId, _importReaddressingSubaddress.OldTerrainObjectHouseNumberId, _importReaddressingSubaddress.OldSubaddressId) }
                            }))
                        .Build(17, EventSerializerSettings))
                });
        }

        public IEventCentricTestSpecificationBuilder RetireNewSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithSubaddressId(NewSubaddressNr16Bus2Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(3))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddressWithNewSubaddressId())
                .When(importSubaddressFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Bus2Id, GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(Gebouw1Id, NewGebouwEenheid1Id, NewGebouwEenheid1Key, NewAddress16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), GebouwEenheid1Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireNewReaddressedSubaddress()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(3))));

            var buildingUnitWasReaddedByOtherUnitRemoval = new BuildingUnitWasReaddedByOtherUnitRemoval(Gebouw1Id, NewGebouwEenheid1Id, NewGebouwEenheid1Key, NewAddress16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), GebouwEenheid1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddressWithNewSubaddressId())
                .When(importSubaddressFromCrab)
                .Then(new Fact[]
                {
                    new Fact(Gebouw1Id, new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid2Id)),
                    new Fact(Gebouw1Id, new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Bus1Id, GebouwEenheid2Id)),
                    new Fact(Gebouw1Id, new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid3Id)),
                    new Fact(Gebouw1Id, buildingUnitWasReaddedByOtherUnitRemoval),
                    new Fact(Gebouw1Id, importSubaddressFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(Gebouw1Id)
                        .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(_importReaddressingSubaddress.TerrainObjectId, _importReaddressingSubaddress.OldTerrainObjectHouseNumberId, _importReaddressingSubaddress.OldSubaddressId), _importReaddressingSubaddress.ToLegacyEvent() }
                        })
                        .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>
                        {
                            { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId), _importReaddressingHouseNumberFromCrab.ToLegacyEvent() }
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId, _importReaddressingHouseNumberFromCrab.NewHouseNumberId },
                            { _importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId, _importReaddressingHouseNumberFromCrab.OldHouseNumberId }
                        })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            {
                                new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_importOldSubaddress.TerrainObjectHouseNumberId, _importOldSubaddress.HouseNumberId),
                                new List<AddressSubaddressWasImportedFromCrab> {_importOldSubaddress.ToLegacyEvent() }
                            },
                            {
                                new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_importNewSubaddress.TerrainObjectHouseNumberId, _importNewSubaddress.HouseNumberId),
                                new List<AddressSubaddressWasImportedFromCrab> { _importNewReaddressedSubaddress.ToLegacyEvent(), _importNewSubaddress.ToLegacyEvent(), importSubaddressFromCrab.ToLegacyEvent() }
                            }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded)
                                    .WithAddressIds(new List<AddressId>())
                                    .WithPreviousAddressId(AddressId.CreateFor(_importReaddressingHouseNumberFromCrab.NewHouseNumberId))
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>{ _buildingUnitWasReaddressed }),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitSubaddressWasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithRetiredBySelf()
                                    .WithPreviousAddressId(AddressId.CreateFor(_importReaddressingSubaddress.NewSubaddressId))
                                    .WithAddressIds(new List<AddressId>())
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed> { _buildingUnitSubaddressWasReaddressed }),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithPreviousAddressId(AddressId.CreateFor(_importReaddressingHouseNumberFromCrab.NewHouseNumberId))
                                    .WithAddressIds(new List<AddressId>()),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitNewSubaddressWasAdded),
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasReaddedByOtherUnitRemoval)
                                    .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>{_buildingUnitWasReaddressed})
                            })
                            .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>
                            {
                                { BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId), BuildingUnitKey.Create(_importReaddressingHouseNumberFromCrab.TerrainObjectId, _importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId) },
                                { BuildingUnitKey.Create(_importReaddressingSubaddress.TerrainObjectId, _importReaddressingSubaddress.NewTerrainObjectHouseNumberId, _importReaddressingSubaddress.NewSubaddressId), BuildingUnitKey.Create(_importReaddressingSubaddress.TerrainObjectId, _importReaddressingSubaddress.OldTerrainObjectHouseNumberId, _importReaddressingSubaddress.OldSubaddressId) }
                            }))
                        .Build(21, EventSerializerSettings))
                });
        }

        [Fact]
        public void AddReaddressingSubaddressTest()
        {
            Assert(AddReaddressingOfSubaddress());
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
        public void AddReaddressedSubaddressWithNewSubaddressIdTest()
        {
            Assert(AddReaddressedSubaddressWithNewSubaddressId());
        }

        [Fact]
        public void AddNewSubaddress2()
        {
            Assert(AddNewSubaddressWithNewSubaddressId());
        }

        [Fact]
        public void RetireOldHouseNrTest()
        {
            Assert(RetireOldHouseNr());
        }

        [Fact]
        public void RetireOldSubaddressTest()
        {
            Assert(RetireOldSubaddress());
        }

        [Fact]
        public void RetireNewSubaddressTest()
        {
            Assert(RetireNewSubaddress());
        }

        [Fact]
        public void RetireNewReaddressedSubaddressTest()
        {
            Assert(RetireNewReaddressedSubaddress());
        }
    }
}

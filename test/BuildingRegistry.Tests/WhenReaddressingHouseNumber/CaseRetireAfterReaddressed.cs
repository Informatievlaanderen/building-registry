namespace BuildingRegistry.Tests.Cases
{
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
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class CaseRetireAfterReaddressed : SnapshotBasedTest
    {
        private LocalDate _readdressingBeginDate;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId OldHuisNr16KoppelingId { get; }
        private CrabHouseNumberId OldHuisNr16Id { get; }

        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, OldHuisNr16KoppelingId);

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);

        public CaseRetireAfterReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(Fixture.Create<int>());
            OldHuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            NewHuisNr16Id = new CrabHouseNumberId(Fixture.Create<int>());
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressingBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            ImportReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(ImportReaddressingHouseNumber)
                .Then(Gebouw1Id, ImportReaddressingHouseNumber.ToLegacyEvent());
        }

        private ImportReaddressingHouseNumberFromCrab ImportReaddressingHouseNumber { get; set; }
        private BuildingUnitWasAdded BuildingUnitWasAdded { get; set; }
        private BuildingUnitWasReaddressed BuildingUnitWasReaddressed { get; set; }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id);

            BuildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key,
                OldAddress16Id,
                new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            BuildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id,
                NewAddress16Id,
                ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    BuildingUnitWasAdded,
                    BuildingUnitWasReaddressed,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent(
                    ));
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));


            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(new[]
                {
                    new Fact(Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(Gebouw1Id)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                                {
                                    {
                                        ImportReaddressingHouseNumber.NewTerrainObjectHouseNumberId,
                                        ImportReaddressingHouseNumber.NewHouseNumberId
                                    },
                                    {
                                        ImportReaddressingHouseNumber.OldTerrainObjectHouseNumberId,
                                        ImportReaddressingHouseNumber.OldHouseNumberId
                                    },
                                })
                            .WithHouseNumberReaddressedEventsByBuildingUnit(
                                new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                                {
                                    {GebouwEenheid1Key, ImportReaddressingHouseNumber.ToLegacyEvent()}
                                })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                                {
                                    ImportReaddressingHouseNumber.OldTerrainObjectHouseNumberId,
                                    ImportReaddressingHouseNumber.NewTerrainObjectHouseNumberId
                                }
                            )
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(BuildingUnitWasAdded)
                                            .WithAddressIds(new List<AddressId>
                                            {
                                                new AddressId(BuildingUnitWasReaddressed.NewAddressId)
                                            })
                                            .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                            {
                                                BuildingUnitWasReaddressed
                                            })
                                    })
                                    .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>()
                                    {
                                        {
                                            BuildingUnitKey.Create(ImportReaddressingHouseNumber.TerrainObjectId,
                                                ImportReaddressingHouseNumber.NewTerrainObjectHouseNumberId),
                                            BuildingUnitKey.Create(ImportReaddressingHouseNumber.TerrainObjectId,
                                                ImportReaddressingHouseNumber.OldTerrainObjectHouseNumberId)
                                        }
                                    })
                            )
                            .WithLastModificationFromCrab(Modification.Update)
                            .Build(5, EventSerializerSettings))
                });
        }

        public IEventCentricTestSpecificationBuilder RetireOldHouseNr()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(2))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(new[]
                {
                    new Fact(Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(Gebouw1Id)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                                {
                                    {
                                        ImportReaddressingHouseNumber.NewTerrainObjectHouseNumberId,
                                        ImportReaddressingHouseNumber.NewHouseNumberId
                                    },
                                    {
                                        importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId,
                                        importTerrainObjectHouseNumberFromCrab.HouseNumberId
                                    }
                                })
                            .WithHouseNumberReaddressedEventsByBuildingUnit(
                                new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                                {
                                    {GebouwEenheid1Key, ImportReaddressingHouseNumber.ToLegacyEvent()}
                                })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                                {importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId})
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(BuildingUnitWasAdded)
                                            .WithAddressIds(new List<AddressId>
                                            {
                                                new AddressId(BuildingUnitWasReaddressed.NewAddressId)
                                            })
                                            .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                            {
                                                BuildingUnitWasReaddressed
                                            })
                                    })
                                    .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>()
                                    {
                                        {
                                            BuildingUnitKey.Create(ImportReaddressingHouseNumber.TerrainObjectId,
                                                ImportReaddressingHouseNumber.NewTerrainObjectHouseNumberId),
                                            BuildingUnitKey.Create(ImportReaddressingHouseNumber.TerrainObjectId,
                                                ImportReaddressingHouseNumber.OldTerrainObjectHouseNumberId)
                                        }
                                    })
                            )
                            .WithLastModificationFromCrab(Modification.Update)
                            .Build(5, EventSerializerSettings))
                });
        }

        [Fact]
        public void AddReaddressingHouseNumberTest()
        {
            Assert(AddReaddressingOfHouseNumber());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberIdTest()
        {
            Assert(AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId());
        }

        [Fact]
        public void RetireOldHouseNrTest()
        {
            Assert(RetireOldHouseNr());
        }
    }
}

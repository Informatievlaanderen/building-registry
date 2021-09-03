namespace BuildingRegistry.Tests.WhenReaddressingHouseNumber
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
    using Xunit;
    using Xunit.Abstractions;

    public class GivenReaddressingBeforeHouseNumberRetire : SnapshotBasedTest
    {

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId OldHuisNr16KoppelingId { get; }
        private CrabHouseNumberId OldHuisNr16Id { get; }

        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);
        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, OldHuisNr16KoppelingId);
        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
        private ReaddressingBeginDate ReaddressBeginDate { get; }

        private BuildingUnitWasReaddressed buildingUnitWasReaddressed;
        private BuildingUnitWasAdded buildingUnitWasAdded;

        private BuildingUnitWasReaddressed buildingUnitWasReaddressed1;
        private BuildingUnitWasAdded buildingUnitWasAdded1;

        private ImportReaddressingHouseNumberFromCrab importReaddressingHouseNumberFromCrab;


        public GivenReaddressingBeforeHouseNumberRetire(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(16);
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
            OldHuisNr16Id = new CrabHouseNumberId(161616);
            NewHuisNr16Id = new CrabHouseNumberId(171717);
            ReaddressBeginDate = new ReaddressingBeginDate(Fixture.Create<LocalDate>());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressBeginDate);

            importReaddressingHouseNumberFromCrab = importReaddressingHouseNumber;

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(-6))));

            buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key,
                OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));

            buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id,
                NewAddress16Id, ReaddressBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded,
                    buildingUnitWasReaddressed,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(-5))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder UnRetireHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(-4))));

            buildingUnitWasAdded1 = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1IdV2, GebouwEenheid1Key,
                OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp),
                GebouwEenheid1Id);

            buildingUnitWasReaddressed1 = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1IdV2, OldAddress16Id,
                NewAddress16Id, ReaddressBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(RetireHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded1,
                    buildingUnitWasReaddressed1,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(UnRetireHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                // .Then(Gebouw1Id,
                //     importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()
                // );
                .Then(new[]
                {
                    new Fact(Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(Gebouw1Id)
                            .WithLastModificationFromCrab(Modification.Update)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                                {
                                    {
                                        importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId,
                                        importTerrainObjectHouseNumberFromCrab.HouseNumberId
                                    },
                                    {
                                        importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId,
                                        importReaddressingHouseNumberFromCrab.OldHouseNumberId
                                    }
                                })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                                {
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId,
                                    importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId
                                }
                            )
                            .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                            {
                                {BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId), importReaddressingHouseNumberFromCrab.ToLegacyEvent()}
                            })
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                            .WithAddressIds(new List<AddressId>())
                                            .WithPreviousAddressId(new AddressId(buildingUnitWasReaddressed.NewAddressId))
                                            .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                            {
                                                buildingUnitWasReaddressed,
                                            })
                                            .WithStatus(BuildingUnitStatus.NotRealized),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1)
                                        .WithAddressIds(new List<AddressId>()
                                        {
                                            new AddressId(buildingUnitWasReaddressed1.NewAddressId)
                                        })
                                        .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                        {
                                        buildingUnitWasReaddressed1,
                                    })
                                    })
                                    .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>()
                                    {
                                        {
                                            BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                                importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId),
                                            BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                                importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId)
                                        }
                                    })
                            )
                            .Build(11, EventSerializerSettings))
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
        public void RetireOldHouseNumberTest()
        {
            Assert(RetireHouseNumber());
        }

        [Fact]
        public void UnretireOldHouseNumberTest()
        {
            Assert(UnRetireHouseNumber());
        }

        [Fact]
        public void AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberIdTest()
        {
            Assert(AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId());
        }


    }
}

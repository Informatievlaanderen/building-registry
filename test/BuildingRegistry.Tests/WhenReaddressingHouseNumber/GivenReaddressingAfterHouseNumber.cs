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

    public class GivenReaddressingAfterHouseNumber : SnapshotBasedTest
    {

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

        private BuildingUnitWasReaddressed buildingUnitWasReaddressed;
        private BuildingUnitWasAdded buildingUnitWasAdded;
        private ImportReaddressingHouseNumberFromCrab importReaddressingHouseNumberFromCrab;
        public GivenReaddressingAfterHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(Fixture.Create<int>());
            OldHuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            NewHuisNr16Id = new CrabHouseNumberId(Fixture.Create<int>());
            ReaddressingBeginDate = new ReaddressingBeginDate(Fixture.Create<LocalDate>());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id);

            buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key,
                OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            importReaddressingHouseNumberFromCrab = importReaddressingHouseNumber;

            buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id,
                GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    buildingUnitWasReaddressed,
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
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
                                            .WithAddressIds(new List<AddressId>()
                                            {
                                                new AddressId(buildingUnitWasReaddressed.NewAddressId)
                                            })
                                            .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                            {
                                                buildingUnitWasReaddressed,
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
                            .Build(5, EventSerializerSettings))
                });
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddReaddressingHouseNumberTest()
        {
            Assert(AddReaddressingOfHouseNumber());
        }

        [Fact]
        public void AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberIdTest()
        {
            Assert(AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId());
        }
    }
}

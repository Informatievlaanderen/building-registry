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
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStatusBeforeReaddressHouseNumber : SnapshotBasedTest
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

        private ImportReaddressingHouseNumberFromCrab importReaddressingHouseNumberFromCrab;
        private ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab;
        private BuildingUnitWasReaddressed buildingUnitWasReaddressed;
        private BuildingUnitWasAdded buildingUnitWasAdded;

        public GivenStatusBeforeReaddressHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

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
                .WithHouseNumberId(OldHuisNr16Id);

            buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key,
                OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded,
                    buildingUnitWasReaddressed,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusForOldHouseNr()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.InUse)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate) ReaddressingBeginDate).ToDateTimeUnspecified().AddDays(-2))));

            importHouseNumberStatusFromCrab = importStatus;

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid1Id),
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(SetStatusForOldHouseNr())
                .When(importTerrainObjectHouseNumberFromCrab)
                // .Then(Gebouw1Id,
                //     importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
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
                            .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                            {
                                {BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId), importReaddressingHouseNumberFromCrab.ToLegacyEvent()}
                            })
                            .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>()
                            {
                                {
                                    new AddressId(buildingUnitWasReaddressed.OldAddressId),
                                    new List<AddressHouseNumberStatusWasImportedFromCrab>()
                                    {
                                        importHouseNumberStatusFromCrab.ToLegacyEvent()
                                    }
                                }
                            })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                                {
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId,
                                    importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId
                                }
                            )
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
                                            .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>()
                                            {
                                                importHouseNumberStatusFromCrab.ToLegacyEvent()
                                            })
                                            .WithStatus(BuildingUnitStatus.Realized),

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
                            .Build(7, EventSerializerSettings))
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
        public void SetStatusForOldHouseNrTest()
        {
            Assert(SetStatusForOldHouseNr());
        }

        [Fact]
        public void AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberIdTest()
        {
            Assert(AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId());
        }
    }
}

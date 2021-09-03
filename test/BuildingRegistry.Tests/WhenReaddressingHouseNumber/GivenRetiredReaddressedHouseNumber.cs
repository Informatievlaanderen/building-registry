namespace BuildingRegistry.Tests.WhenReaddressingHouseNumber
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
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRetiredReaddressedHouseNumber : SnapshotBasedTest
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

        private BuildingUnitKey GebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 2);
        private BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid2Key, 3);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);

        private BuildingUnitWasAdded buildingUnitWasAdded1;
        private BuildingUnitWasAdded buildingUnitWasAdded2;
        private BuildingUnitWasAdded buildingUnitWasAdded3;
        private BuildingUnitWasReaddressed buildingUnitWasReaddressed;
        private ImportReaddressingHouseNumberFromCrab importReaddressingHouseNumberFromCrab;
        private ImportHouseNumberStatusFromCrab importHouseNumberStatusFromCrab;

        public GivenRetiredReaddressedHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

            buildingUnitWasAdded1 = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key,
                OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded1,
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

            buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id,
                NewAddress16Id, ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    buildingUnitWasReaddressed,
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireWithNewHouseNumberId()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ReaddHouseNumberWithNewHouseNrId()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            buildingUnitWasAdded2 = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, GebouwEenheid2Key,
                NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp),
                GebouwEenheid1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(RetireWithNewHouseNumberId())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded2,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusWithNewHouseNumberId()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Reserved);

            importHouseNumberStatusFromCrab = importStatus;

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddHouseNumberWithNewHouseNrId())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid2Id),
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireWithNewHouseNumberAgain()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(SetStatusWithNewHouseNumberId())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid2Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder UnretireWithNewHouseNumberAgain()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(2))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            buildingUnitWasAdded3 = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid2Key,
                NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp),
                GebouwEenheid2Id);
            return new AutoFixtureScenario(Fixture)
                .Given(RetireWithNewHouseNumberAgain())
                .When(importTerrainObjectHouseNumberFromCrab)
                // .Then(Gebouw1Id,
                //     buildingUnitWasAdded3,
                //     new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid3Id),
                //     importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()
                // );
                .Then(new[]
                {
                    new Fact(Gebouw1Id, buildingUnitWasAdded3),
                    new Fact(Gebouw1Id, new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid3Id)),
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
                                    }
                                })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                                {
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId,
                                    importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId
                                }
                            )
                            .WithHouseNumberReaddressedEventsByBuildingUnit(
                                new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                                {
                                    {
                                        BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                            importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId),
                                        importReaddressingHouseNumberFromCrab.ToLegacyEvent()
                                    }
                                })
                            .WithHouseNumberReaddressedEventsByBuildingUnit(
                                new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                                {
                                    {
                                        BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                            importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId),
                                        importReaddressingHouseNumberFromCrab.ToLegacyEvent()
                                    }
                                })
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1)
                                            .WithAddressIds(new List<AddressId>())
                                            .WithPreviousAddressId(new AddressId(buildingUnitWasReaddressed.NewAddressId))
                                            .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                            {
                                                buildingUnitWasReaddressed,
                                            })
                                            .WithStatus(BuildingUnitStatus.NotRealized),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded2)
                                            .WithAddressIds(new List<AddressId>())
                                            .WithPreviousAddressId(new AddressId(buildingUnitWasReaddressed.NewAddressId))
                                            .WithStatus(BuildingUnitStatus.NotRealized)
                                            .WithHouseNumberStatusChronicle(
                                                new List<AddressHouseNumberStatusWasImportedFromCrab>()
                                                {
                                                    importHouseNumberStatusFromCrab.ToLegacyEvent()
                                                }),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded3)
                                            .WithHouseNumberStatusChronicle(
                                                new List<AddressHouseNumberStatusWasImportedFromCrab>()
                                                {
                                                    importHouseNumberStatusFromCrab.ToLegacyEvent()
                                                })
                                            .WithStatus(BuildingUnitStatus.Planned)
                                    })
                                    .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>()
                                    {
                                        {
                                            BuildingUnitKey.Create(
                                                importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                                importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId),
                                            BuildingUnitKey.Create(
                                                importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                                importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId)
                                        }
                                    })
                            )
                            .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>()
                            {
                                {
                                    new AddressId(buildingUnitWasReaddressed.NewAddressId),
                                    new List<AddressHouseNumberStatusWasImportedFromCrab>()
                                    {
                                        importHouseNumberStatusFromCrab.ToLegacyEvent()
                                    }
                                }
                            })
                            .Build(18, EventSerializerSettings))
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

        [Fact] //TODO: *FLICKERING TEST* sometimes it crashes, check id's
        public void RetireNewHouseNumberTest()
        {
            Assert(RetireWithNewHouseNumberId());
        }

        [Fact]
        public void ReaddRetiredHouseNumberTest()
        {
            Assert(ReaddHouseNumberWithNewHouseNrId());
        }

        [Fact]
        public void SetStatusForNewBuildingUnit()
        {
            Assert(SetStatusWithNewHouseNumberId());
        }

        [Fact]
        public void RetireWithNewHouseNumberAgainTest()
        {
            Assert(RetireWithNewHouseNumberAgain());
        }

        [Fact]
        public void UnretireWithNewHouseNumberAgainTest()
        {
            Assert(UnretireWithNewHouseNumberAgain());
        }
    }
}

namespace BuildingRegistry.Tests.WhenReaddressingHouseNumber
{
    using System;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using ValueObjects;
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRetiredReaddressedHouseNumber : AutofacBasedTest
    {
        protected readonly IFixture Fixture;

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
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);

        public GivenRetiredReaddressedHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
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

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
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

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressingBeginDate),
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
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(RetireWithNewHouseNumberId())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, GebouwEenheid2Key, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusWithNewHouseNumberId()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Reserved);

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddHouseNumberWithNewHouseNrId())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid2Id),
                    importStatus.ToLegacyEvent());
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
    }
}

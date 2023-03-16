namespace BuildingRegistry.Tests.Legacy.WhenReaddressingHouseNumber
{
    using System;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Fixtures;
    using NodaTime;
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStatusAfterReaddressHouseNumber : AutofacBasedTest
    {
        protected readonly IFixture Fixture;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId OldHuisNr16KoppelingId { get; }
        private CrabHouseNumberId OldHuisNr16Id { get; }

        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);
        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, OldHuisNr16KoppelingId);
        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
        private ReaddressingBeginDate ReaddressBeginDate { get; }

        public GivenStatusAfterReaddressHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
            OldHuisNr16Id = new CrabHouseNumberId(161616);
            NewHuisNr16Id = new CrabHouseNumberId(171717);
            ReaddressBeginDate = new ReaddressingBeginDate(Fixture.Create<LocalDate>());
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

        public IEventCentricTestSpecificationBuilder SetHouseNumberStatus()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithStatus(CrabAddressStatus.InUse);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid1Id),
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(SetHouseNumberStatus())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressBeginDate),
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusWithNewHouseNumberId()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Reserved);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid1Id),
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetStatusWithOldHouseNumberId()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(3)), null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(2))))
                .WithStatus(CrabAddressStatus.InUse);

            return new AutoFixtureScenario(Fixture)
                .Given(SetStatusWithNewHouseNumberId())
                .When(importStatus)
                .Then(Gebouw1Id,
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSameStatusWithNewHouseNumberIdButBeforeStatusOldHouseNr()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(2))))
                .WithStatus(CrabAddressStatus.Reserved);

            return new AutoFixtureScenario(Fixture)
                .Given(SetStatusWithOldHouseNumberId())
                .When(importStatus)
                .Then(Gebouw1Id,
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetRetiredStatusWithOldHouseNumberId()
        {
            var importStatus = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(3)), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(2))))
                .WithHouseNumberId(OldHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(SetStatusWithNewHouseNumberId())
                .When(importStatus)
                .Then(Gebouw1Id,
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSameStatusWithNewHouseNumberIdButBeforeStatusOldHouseNrBasedOnRetired()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(((LocalDate)ReaddressBeginDate).ToDateTimeUnspecified().AddDays(2))))
                .WithStatus(CrabAddressStatus.Reserved);

            return new AutoFixtureScenario(Fixture)
                .Given(SetRetiredStatusWithOldHouseNumberId())
                .When(importStatus)
                .Then(Gebouw1Id,
                    importStatus.ToLegacyEvent());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void SetHouseNumberStatusTest()
        {
            Assert(SetHouseNumberStatus());
        }

        [Fact]
        public void AddReaddressingHouseNumberTest()
        {
            Assert(AddReaddressingOfHouseNumber());
        }

        [Fact]
        public void AddReaddressedHouseNumberStatusWithNewHouseNumberIdTest()
        {
            Assert(SetStatusWithNewHouseNumberId());
        }

        [Fact]
        public void AddReaddressedOldHouseNumberStatusWithOldHouseNumberTest()
        {
            Assert(SetStatusWithOldHouseNumberId());
        }

        [Fact]
        public void AddReaddressedOldHouseNumberRetiredStatusWithOldHouseNumberTest()
        {
            Assert(SetRetiredStatusWithOldHouseNumberId());
        }

        [Fact]
        public void SetSameStatusWithNewHouseNumberIdButBeforeStatusOldHouseNrTest()
        {
            Assert(SetSameStatusWithNewHouseNumberIdButBeforeStatusOldHouseNr());
        }

        [Fact]
        public void SetSameStatusWithNewHouseNumberIdButBeforeStatusOldHouseNrBasedOnRetriedTest()
        {
            Assert(SetSameStatusWithNewHouseNumberIdButBeforeStatusOldHouseNrBasedOnRetired());
        }

        //TODO: create test which adds status to new housenumber while old is still active => should not change status (OK i think)
        //TODO: followed by status change from old housenr that could be influence by previously added status CRAB Event should be added to unit Chronicle but not influenced by it. Until new housenr becomes active.
    }

    public class Test : AutofacBasedTest
    {
        protected readonly IFixture Fixture;
        private readonly LocalDate _readdressingBeginDate;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId OldHuisNr16KoppelingId { get; }
        private CrabHouseNumberId OldHuisNr16Id { get; }

        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);
        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, OldHuisNr16KoppelingId);
        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
        private ReaddressingBeginDate ReaddressBeginDate { get; }

        public Test(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(Fixture.Create<int>() + OldHuisNr16KoppelingId);
            OldHuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            NewHuisNr16Id = new CrabHouseNumberId(Fixture.Create<int>() + OldHuisNr16Id);
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-4))));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetHouseNumberStatus()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithStatus(CrabAddressStatus.InUse)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-4))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid1Id),
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(SetHouseNumberStatus())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressBeginDate),
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetHouseNumberStatusAgain()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Proposed)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-3))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasPlanned(Gebouw1Id, GebouwEenheid1Id),
                    importStatus.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewTerrainObjectHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(SetHouseNumberStatusAgain())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetHouseNumberStatusForNewHouseNr()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithStatus(CrabAddressStatus.OutOfUse)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(SetHouseNumberStatusAgain())
                .When(importStatus)
                .Then(Gebouw1Id,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid1Id),
                    importStatus.ToLegacyEvent());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void SetHouseNumberStatusTest()
        {
            Assert(SetHouseNumberStatus());
        }

        [Fact]
        public void AddReaddressingHouseNumberTest()
        {
            Assert(AddReaddressingOfHouseNumber());
        }

        [Fact]
        public void AddStatusAgainTest()
        {
            Assert(SetHouseNumberStatusAgain());
        }

        [Fact]
        public void AddNewTerrainObjectHouseNrTest()
        {
            Assert(AddNewTerrainObjectHouseNumber());
        }

        [Fact]
        public void SetStatusForNewHouseNrButNotReaddressedYetTest()
        {
            Assert(SetHouseNumberStatusForNewHouseNr());
        }
    }
}

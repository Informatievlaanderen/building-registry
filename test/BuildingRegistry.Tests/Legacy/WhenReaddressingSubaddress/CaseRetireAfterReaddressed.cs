namespace BuildingRegistry.Tests.Legacy.WhenReaddressingSubaddress
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using NodaTime;
    using Tests.Autofixture;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class CaseRetireAfterReaddressed : AutofacBasedTest
    {
        protected readonly IFixture Fixture;
        private LocalDate _readdressingBeginDate;

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
            Fixture = new Fixture()
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
            var importReaddressingHouseNumberFromCrab = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldHouseNumberId(HuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importReaddressingHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    importReaddressingHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            var importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNr())
                .When(importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, Address16Id, NewAddress16Id, ReaddressingBeginDate),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithHouseNumberId(HuisNr16Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importSubaddress)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedSubaddressWithNewSubaddressId()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(importSubaddress)
                .Then(Gebouw1Id,
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddressWithNewSubaddressId()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus2Id)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(2))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressedSubaddressWithNewSubaddressId())
                .When(importSubaddress)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid4Id, GebouwEenheid4Key, NewAddress16Bus2Id, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(Gebouw1Id, NewAddress16Id, GebouwEenheid3Id),
                    importSubaddress.ToLegacyEvent());
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
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id)
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(3))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddressWithNewSubaddressId())
                .When(importSubaddressFromCrab)
                .Then(Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
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
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(3))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddressWithNewSubaddressId())
                .When(importSubaddressFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasNotRealized(Gebouw1Id, GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Bus1Id, GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(Gebouw1Id, NewAddress16Id, GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(Gebouw1Id, NewGebouwEenheid1Id, NewGebouwEenheid1Key, NewAddress16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), GebouwEenheid1Id),
                    importSubaddressFromCrab.ToLegacyEvent());
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

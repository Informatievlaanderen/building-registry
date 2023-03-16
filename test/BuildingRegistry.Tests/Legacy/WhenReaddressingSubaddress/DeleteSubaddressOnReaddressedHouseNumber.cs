namespace BuildingRegistry.Tests.Legacy.WhenReaddressingSubaddress
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Fixtures;
    using NodaTime;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Delete readdressed subaddress on readdressed housenumber with active new subaddress
    /// </summary>
    public class DeleteSubaddressOnReaddressedHouseNumber : AutofacBasedTest
    {
        public DeleteSubaddressOnReaddressedHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16))
                ;

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

                NewHuisNr16Id = new CrabHouseNumberId(customizedFixture.Create<int>() + HuisNr16Id);
                NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>() + HuisNr16KoppelingId);
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

        protected readonly IFixture Fixture;
        protected TestCase1AData _ { get; }

        public IEventCentricTestSpecificationBuilder ReaddressHouseNumber()
        {
            var importReaddressingHouseNumberFromCrab = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(_.NewHuisNr16Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importReaddressingHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importReaddressingHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ReaddressSubaddress()
        {
            var importReaddressingSubaddressFromCrab = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldSubaddressId(_.SubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddressHouseNumber())
                .When(importReaddressingSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importReaddressingSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportOldSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(ReaddressSubaddress())
                .When(importSubaddress)
                .Then(_.Gebouw1Id,
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberWithImportedSubaddressUnits()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(ImportOldSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2Id, _.Address16Bus1Id, _.NewAddress16Bus1Id, _.ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberWithImportedSubaddressUnits())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewHouseNumber())
                .When(importSubaddress)
                .Then(_.Gebouw1Id,
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddHours(5))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddHours(6))));

            return new AutoFixtureScenario(Fixture)
                .Given(RetireOldHouseNumber())
                .When(importSubaddress)
                .Then(_.Gebouw1Id,
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewSubaddress2()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.NewSubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(RetireOldSubaddress())
                .When(importSubaddress)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.NewAddress16Bus2Id, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id),
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RemoveReaddressedSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithModification(CrabModification.Delete)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_.ReaddressingBeginLocalDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewSubaddress2())
                .When(importSubaddress)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.NewAddress16Id, new BuildingUnitVersion(importSubaddress.Timestamp), _.GebouwEenheid1Id),
                    importSubaddress.ToLegacyEvent());
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

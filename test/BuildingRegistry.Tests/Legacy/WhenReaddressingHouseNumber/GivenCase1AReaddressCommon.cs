namespace BuildingRegistry.Tests.Legacy.WhenReaddressingHouseNumber
{
    using System.Linq;
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
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCase1AReaddressCommon : AutofacBasedTest
    {
        public GivenCase1AReaddressCommon(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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

                var intGenerator = customizedFixture.Create<Generator<int>>();
                NewHuisNr16Id = new CrabHouseNumberId(intGenerator.First(x => x != HuisNr16Id));
                NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(intGenerator.First(x => x != HuisNr16KoppelingId));

                ReaddressingBeginDate = new ReaddressingBeginDate(customizedFixture.Create<LocalDate>());
            }

            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabHouseNumberId NewHuisNr16Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid5Key => GebouwEenheid1Key;
            public BuildingUnitKey GebouwEenheid6Key => BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid2IdV2 => BuildingUnitId.Create(GebouwEenheid2Key, 2);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid3IdV2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid4IdV2 => BuildingUnitId.Create(GebouwEenheid4Key, 2);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 2);
            public BuildingUnitId GebouwEenheid6Id => BuildingUnitId.Create(GebouwEenheid6Key, 1);

            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address16Bus1Id => AddressId.CreateFor(SubaddressNr16Bus1Id);
            public AddressId Address16Bus2Id => AddressId.CreateFor(SubaddressNr16Bus2Id);
            public AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
            public ReaddressingBeginDate ReaddressingBeginDate { get; }
        }

        protected readonly IFixture Fixture;
        protected TestCase1AData _ { get; }

        public IEventCentricTestSpecificationBuilder T1()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(T1())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3Readdress()
        {
            var importReaddressingHouseNumberFromCrab = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(_.NewHuisNr16Id)
                .WithBeginDate(_.ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importReaddressingHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid3Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate),
                    importReaddressingHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T3Readdress())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.NewAddress16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid1Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T5()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.NewHuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T5())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid5Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder TestReaddHouseNumberWithNewHouseNrId()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, _.NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }



        [Fact]
        public void TestT1()
        {
            Assert(T1());
        }

        [Fact]
        public void TestT2()
        {
            Assert(T2());
        }

        [Fact]
        public void TestT3()
        {
            Assert(T3());
        }

        [Fact]
        public void TestT3Readdress()
        {
            Assert(T3Readdress());
        }

        [Fact]
        public void TestT4()
        {
            Assert(T4());
        }

        [Fact]
        public void TestT5()
        {
            Assert(T5());
        }

        [Fact]
        public void TestT6()
        {
            Assert(T6());
        }

        [Fact]
        public void TestReaddHouseNr()
        {
            Assert(TestReaddHouseNumberWithNewHouseNrId());
        }
    }
}

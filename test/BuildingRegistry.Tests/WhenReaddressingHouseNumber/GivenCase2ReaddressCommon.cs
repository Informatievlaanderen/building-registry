namespace BuildingRegistry.Tests.WhenReaddressingHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using System;
    using ValueObjects;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingSubaddress;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCase2ReaddressCommon : AutofacBasedTest
    {
        protected IFixture Fixture { get; }
        protected TestCase2Data _ { get; }

        public GivenCase2ReaddressCommon(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCase2Data(Fixture);
        }

        protected class TestCase2Data
        {
            public TestCase2Data(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(16);
                NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
                HuisNr18KoppelingId = new CrabTerrainObjectHouseNumberId(18);
                NewHuisNr18KoppelingId = new CrabTerrainObjectHouseNumberId(19);
                HuisNr16Id = new CrabHouseNumberId(161616);
                NewHuisNr16Id = new CrabHouseNumberId(171717);
                HuisNr18Id = new CrabHouseNumberId(181818);
                NewHuisNr18Id = new CrabHouseNumberId(191919);
                SubaddressNr18Bus1Id = new CrabSubaddressId(181);
                NewSubaddressNr18Bus1Id = new CrabSubaddressId(188);
                SubaddressNr18Bus2Id = new CrabSubaddressId(182);
                NewSubaddressNr18Bus2Id = new CrabSubaddressId(189);
                SubaddressNr16Bus1Id = new CrabSubaddressId(161);
                NewSubaddressNr16Bus1Id = new CrabSubaddressId(168);
                SubaddressNr16Bus2Id = new CrabSubaddressId(162);
                NewSubaddressNr16Bus2Id = new CrabSubaddressId(169);
                ReaddressDate = LocalDate.FromDateTime(DateTime.Now);
            }


            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr18KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId NewHuisNr18KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabHouseNumberId NewHuisNr16Id { get; }
            public CrabHouseNumberId HuisNr18Id { get; }
            public CrabHouseNumberId NewHuisNr18Id { get; }
            public CrabSubaddressId SubaddressNr18Bus1Id { get; }
            public CrabSubaddressId NewSubaddressNr18Bus1Id { get; }
            public CrabSubaddressId SubaddressNr18Bus2Id { get; }
            public CrabSubaddressId NewSubaddressNr18Bus2Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }
            public CrabSubaddressId NewSubaddressNr16Bus2Id { get; }
            public LocalDate ReaddressDate { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr18Bus1Id);

            public BuildingUnitKey GebouwEenheid5Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr18Bus2Id);

            public BuildingUnitKey GebouwEenheid6Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid7Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 1);
            public BuildingUnitId GebouwEenheid6Id => BuildingUnitId.Create(GebouwEenheid6Key, 1);
            public BuildingUnitId GebouwEenheid7Id => BuildingUnitId.Create(GebouwEenheid7Key, 1);

            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
            public AddressId Address18Id => AddressId.CreateFor(HuisNr18Id);
            public AddressId NewAddress18Id => AddressId.CreateFor(NewHuisNr18Id);

            public ReaddressingBeginDate ReaddressingBeginDate => new ReaddressingBeginDate(ReaddressDate);
        }

        public IEventCentricTestSpecificationBuilder Readdress16()
        {
            var importReaddressHouseNr = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithBeginDate(_.ReaddressingBeginDate)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithNewHouseNumberId(_.NewHuisNr16Id)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importReaddressHouseNr)
                .Then(_.Gebouw1Id,
                    importReaddressHouseNr.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Readdress18()
        {
            var importReaddressHouseNr = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithBeginDate(_.ReaddressingBeginDate)
                .WithOldHouseNumberId(_.HuisNr18Id)
                .WithNewHouseNumberId(_.NewHuisNr18Id)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(Readdress16())
                .When(importReaddressHouseNr)
                .Then(_.Gebouw1Id,
                    importReaddressHouseNr.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Readdress16B1()
        {
            var importReaddressSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithBeginDate(_.ReaddressingBeginDate)
                .WithOldSubaddressId(_.SubaddressNr16Bus1Id)
                .WithNewSubaddressId(_.NewSubaddressNr16Bus1Id)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(Readdress18())
                .When(importReaddressSubaddress)
                .Then(_.Gebouw1Id,
                    importReaddressSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Readdress16B2()
        {
            var importReaddressSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithBeginDate(_.ReaddressingBeginDate)
                .WithOldSubaddressId(_.SubaddressNr16Bus2Id)
                .WithNewSubaddressId(_.NewSubaddressNr16Bus2Id)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(Readdress16B1())
                .When(importReaddressSubaddress)
                .Then(_.Gebouw1Id,
                    importReaddressSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Readdress18B1()
        {
            var importReaddressSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithBeginDate(_.ReaddressingBeginDate)
                .WithOldSubaddressId(_.SubaddressNr18Bus1Id)
                .WithNewSubaddressId(_.NewSubaddressNr18Bus1Id)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(Readdress16B2())
                .When(importReaddressSubaddress)
                .Then(_.Gebouw1Id,
                    importReaddressSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder Readdress18B2()
        {
            var importReaddressSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithBeginDate(_.ReaddressingBeginDate)
                .WithOldSubaddressId(_.SubaddressNr18Bus2Id)
                .WithNewSubaddressId(_.NewSubaddressNr18Bus2Id)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithNewTerrainObjectHouseNumberId(_.NewHuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(Readdress18B1())
                .When(importReaddressSubaddress)
                .Then(_.Gebouw1Id,
                    importReaddressSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id) //koppel huisnr 16
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(Readdress18B2())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, _.NewAddress16Id, _.ReaddressingBeginDate),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id) //koppel huisnr 18
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-2))));

            return new AutoFixtureScenario(Fixture)
                .Given(T1())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2Id, _.Address18Id, _.NewAddress18Id, _.ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr18Bus1Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-1))));
            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, AddressId.CreateFor(_.SubaddressNr18Bus1Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid4Id, AddressId.CreateFor(_.SubaddressNr18Bus1Id), AddressId.CreateFor(_.NewSubaddressNr18Bus1Id), _.ReaddressingBeginDate),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr18Bus2Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-1))));
            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, AddressId.CreateFor(_.SubaddressNr18Bus2Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid5Id, AddressId.CreateFor(_.SubaddressNr18Bus2Id), AddressId.CreateFor(_.NewSubaddressNr18Bus2Id), _.ReaddressingBeginDate),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress18Id, _.GebouwEenheid2Id),
                    //new AddressWasDetached(_.Gebouw1Id,_.Address18Id,  _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.NewAddress18Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T5()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-1))));
            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, AddressId.CreateFor(_.SubaddressNr16Bus1Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid6Id, AddressId.CreateFor(_.SubaddressNr16Bus1Id), AddressId.CreateFor(_.NewSubaddressNr16Bus1Id), _.ReaddressingBeginDate),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-1))));
            return new AutoFixtureScenario(Fixture)
                .Given(T5())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid7Id, _.GebouwEenheid7Key, AddressId.CreateFor(_.SubaddressNr16Bus2Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid7Id, AddressId.CreateFor(_.SubaddressNr16Bus2Id), AddressId.CreateFor(_.NewSubaddressNr16Bus2Id), _.ReaddressingBeginDate),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.NewAddress16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportTerrainObjectHouseNrAfterMovedToCommonButBeforeReaddress()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id) //koppel huisnr 16
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(T6())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportNewHouseNr18()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr18KoppelingId)
                .WithHouseNumberId(_.NewHuisNr18Id) //koppel huisnr 18
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(T6())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportNewHouseNr16()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.NewHuisNr16KoppelingId)
                .WithHouseNumberId(_.NewHuisNr16Id) //koppel huisnr 16
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_.ReaddressDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(ImportNewHouseNr18())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void TestRaddress16()
        {
            Assert(Readdress16());
        }

        [Fact]
        public void TestRaddress18()
        {
            Assert(Readdress18());
        }

        [Fact]
        public void TestRaddress16B1()
        {
            Assert(Readdress16B1());
        }

        [Fact]
        public void TestRaddress16B2()
        {
            Assert(Readdress16B2());
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
        public void TestImportNewHouseNr18()
        {
            Assert(ImportNewHouseNr18());
        }

        [Fact]
        public void TestImportNewHouseNr16()
        {
            Assert(ImportNewHouseNr16());
        }

        [Fact]
        public void TestImportTerrainObjectHouseNrAfterMovedToCommonButBeforeReaddress()
        {
            Assert(ImportTerrainObjectHouseNrAfterMovedToCommonButBeforeReaddress());
        }
    }
}

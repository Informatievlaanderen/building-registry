namespace BuildingRegistry.Tests.Cases
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using System.Collections.Generic;
    using ValueObjects;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObject;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingHouseNumber;
    using WhenReaddressingSubaddress;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase5CReaddressing : AutofacBasedTest
    {
        public CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }
        public CrabHouseNumberId NewHuisNr16Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }

        public AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);

        public BuildingUnitKey NewGebouweenheid3Key => BuildingUnitKey.Create(_.Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId, _.SubaddressNr16Bus2Id);
        public BuildingUnitId NewGebouweenheid3Id => BuildingUnitId.Create(NewGebouweenheid3Key, 1);
        public BuildingUnitId NewGebouweenheid3IdV2 => BuildingUnitId.Create(NewGebouweenheid3Key, 2);

        public BuildingUnitKey NewGebouweenheid1KeyV2 => BuildingUnitKey.Create(_.Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);
        public BuildingUnitId NewGebouweenheid1IdV2 => BuildingUnitId.Create(NewGebouweenheid1KeyV2, 2);

        public TestCase5CReaddressing(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCase5CData(_fixture);

            NewSubaddressNr16Bus1Id = new CrabSubaddressId(_fixture.Create<int>());
            NewHuisNr16Id = new CrabHouseNumberId(_fixture.Create<int>());
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(_fixture.Create<int>());
            ReaddressingBeginDate = new ReaddressingBeginDate(_fixture.Create<LocalDate>());
        }

        protected class TestCase5CData
        {
            public TestCase5CData(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNr16KoppelingId = customizedFixture.Create<CrabTerrainObjectHouseNumberId>();
                HuisNr18KoppelingId = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                HuisNr18Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                SubaddressNr16Bus1Id = new CrabSubaddressId(customizedFixture.Create<int>());
                SubaddressNr16Bus2Id = new CrabSubaddressId(customizedFixture.Create<int>());
            }

            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr18KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabHouseNumberId HuisNr18Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId);

            public BuildingUnitKey CommonGebouwEenheidKey =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId);

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid2IdV2 => BuildingUnitId.Create(GebouwEenheid2Key, 2);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid3IdV2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid4IdV2 => BuildingUnitId.Create(GebouwEenheid4Key, 2);
            public BuildingUnitId CommonGebouwEenheidId => BuildingUnitId.Create(CommonGebouwEenheidKey, 1);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address18Id => AddressId.CreateFor(HuisNr18Id);
            public AddressId Address16Bus1Id => AddressId.CreateFor(SubaddressNr16Bus1Id);
            public AddressId Address16Bus2Id => AddressId.CreateFor(SubaddressNr16Bus2Id);
        }

        protected readonly IFixture _fixture;
        protected TestCase5CData _ { get; }

        public IEventCentricTestSpecificationBuilder T0()
        {
            var importTerrainObjectFromCrab = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importTerrainObjectFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasNotRealized(_.Gebouw1Id, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    importTerrainObjectFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(_fixture)
                .Given(T0())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAddedToRetiredBuilding(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealizedByBuilding(_.Gebouw1Id, _.GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(_fixture)
                .Given(T1())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAddedToRetiredBuilding(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealizedByBuilding(_.Gebouw1Id, _.GebouwEenheid2Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2ReaddressHouseNr()
        {
            var readdressHouseNumber = _fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(_fixture)
                .Given(T2())
                .When(readdressHouseNumber)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, NewAddress16Id, ReaddressingBeginDate),
                    readdressHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2ReaddressSubaddress()
        {
            var readdressHouseNumber = _fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithOldSubaddressId(_.SubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT2ReaddressHouseNr())
                .When(readdressHouseNumber)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2Id, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    readdressHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT2ReaddressSubaddress())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAddedToRetiredBuilding(_.Gebouw1Id, NewGebouweenheid3Id, NewGebouweenheid3Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealizedByBuilding(_.Gebouw1Id, NewGebouweenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id);

            return new AutoFixtureScenario(_fixture)
                .Given(T3())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAddedToRetiredBuilding(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealizedByBuilding(_.Gebouw1Id, _.GebouwEenheid4Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T5()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(_fixture)
                .Given(T4())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, NewGebouweenheid3Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6_ByT4()
        {
            var importTerrainObjectFromCrab = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            return new AutoFixtureScenario(_fixture)
                .Given(T4())
                .When(importTerrainObjectFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingStatusWasRemoved(_.Gebouw1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.Address16Id, NewAddress16Id, ReaddressingBeginDate),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(importTerrainObjectFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.CommonGebouwEenheidId, _.CommonGebouwEenheidKey, new BuildingUnitVersion(importTerrainObjectFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.CommonGebouwEenheidId),
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid3IdV2, NewGebouweenheid3Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectFromCrab.Timestamp), NewGebouweenheid3Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, NewAddress16Id, _.CommonGebouwEenheidId),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectFromCrab.Timestamp), _.GebouwEenheid4Id),
                    importTerrainObjectFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T6()
        {
            var importTerrainObjectFromCrab = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), null));

            return new AutoFixtureScenario(_fixture)
                .Given(T5())
                .When(importTerrainObjectFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingStatusWasRemoved(_.Gebouw1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectFromCrab.Timestamp), _.GebouwEenheid4Id),
                    importTerrainObjectFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T7()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(_fixture)
                .Given(T6())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid1IdV2, NewGebouweenheid1KeyV2, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.CommonGebouwEenheidId, _.CommonGebouwEenheidKey, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.CommonGebouwEenheidId),
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid3IdV2, NewGebouweenheid3Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), NewGebouweenheid3Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, NewAddress16Id, _.CommonGebouwEenheidId),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void TestT0()
        {
            Assert(T0());
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
        public void BasedOnT2ReaddressHouseNumberTest()
        {
            Assert(BasedOnT2ReaddressHouseNr());
        }

        [Fact]
        public void BasedOnT2ReaddressSubaddressTest()
        {
            Assert(BasedOnT2ReaddressSubaddress());
        }

        [Fact]
        public void TestT3BasedOnReaddresses()
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
        public void TestT6_StartFromT4()
        {
            Assert(T6_ByT4());
        }

        [Fact]
        public void TestT6()
        {
            Assert(T6());
        }

        [Fact]
        public void TestT7()
        {
            Assert(T7());
        }
    }
}

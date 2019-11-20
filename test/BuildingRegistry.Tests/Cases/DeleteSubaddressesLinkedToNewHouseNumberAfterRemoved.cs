namespace BuildingRegistry.Tests.Cases
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
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// terreinobject id: 7050782
    ///select * from odb.tblTerreinObject_huisNummer where terreinObjectId =7050782
    ///select * from cdb.tblterreinobject_huisnummer_hist where terreinObjectId = 7050782
    ///
    ///select * from odb.tblSubAdres where huisnummerid in (3258112, 2102736)
    ///select * from cdb.tblSubAdres_hist where huisnummerid in (3258112, 2102736)
    /// </summary>
    public class DeleteSubaddressesLinkedToNewHouseNumberAfterRemoved : AutofacBasedTest
    {
        public DeleteSubaddressesLinkedToNewHouseNumberAfterRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCaseData(Fixture);
        }

        protected class TestCaseData
        {
            public TestCaseData(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNrKoppeling1Id = customizedFixture.Create<CrabTerrainObjectHouseNumberId>();
                HuisNrKoppeling2Id = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                HuisNrKoppeling3Id = new CrabTerrainObjectHouseNumberId(customizedFixture.Create<int>());
                HuisNr15Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                HuisNr17Id = new CrabHouseNumberId(customizedFixture.Create<int>());
                SubaddressNr16Bus1Id = new CrabSubaddressId(161);
                SubaddressNr16Bus2Id = new CrabSubaddressId(162);
                SubaddressNr16Bus3Id = new CrabSubaddressId(163);
                SubaddressNr16Bus4Id = new CrabSubaddressId(164);
            }


            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNrKoppeling1Id { get; } //3707305
            public CrabTerrainObjectHouseNumberId HuisNrKoppeling2Id { get; } //3705627
            public CrabTerrainObjectHouseNumberId HuisNrKoppeling3Id { get; } //8127734
            public CrabHouseNumberId HuisNr15Id { get; } //1229249
            public CrabHouseNumberId HuisNr16Id { get; } //2102736
            public CrabHouseNumberId HuisNr17Id { get; } //3258112
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }
            public CrabSubaddressId SubaddressNr16Bus3Id { get; }
            public CrabSubaddressId SubaddressNr16Bus4Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling2Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid5Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid6Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus3Id);

            public BuildingUnitKey GebouwEenheid7Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling1Id, SubaddressNr16Bus4Id);

            public BuildingUnitKey GebouwEenheid8Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNrKoppeling3Id);

            //public BuildingUnitKey GebouwEenheid9Key =>
            //    BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr16Bus4Id);

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid3IdV2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 1);
            public BuildingUnitId GebouwEenheid5IdV2 => BuildingUnitId.Create(GebouwEenheid5Key, 2);
            public BuildingUnitId GebouwEenheid6Id => BuildingUnitId.Create(GebouwEenheid6Key, 1);
            public BuildingUnitId GebouwEenheid7Id => BuildingUnitId.Create(GebouwEenheid7Key, 1);
            public BuildingUnitId GebouwEenheid8Id => BuildingUnitId.Create(GebouwEenheid8Key, 1);
            //public BuildingUnitId GebouwEenheid9Id => BuildingUnitId.Create(GebouwEenheid9Key, 1);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address15Id => AddressId.CreateFor(HuisNr15Id);
            public AddressId Address17Id => AddressId.CreateFor(HuisNr17Id);
        }

        protected IFixture Fixture { get; }
        protected TestCaseData _ { get; }


        public IEventCentricTestSpecificationBuilder AddHouseNumber15()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)))
                .WithHouseNumberId(_.HuisNr15Id);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address15Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumber16()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling2Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)))
                .WithHouseNumberId(_.HuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumber15())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress1WithFutureHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumber16())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id, importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress2WithFutureHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress1WithFutureHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id, importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress3WithFutureHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus3Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress2WithFutureHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id, importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress4WithFutureHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus4Id)
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithHouseNumberId(_.HuisNr17Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress3WithFutureHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id, importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSwitchHouseNumber15ToHouseNumber17()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(2))))
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithHouseNumberId(_.HuisNr17Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress4WithFutureHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address17Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, AddressId.CreateFor(_.SubaddressNr16Bus1Id), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, AddressId.CreateFor(_.SubaddressNr16Bus2Id), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address17Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address17Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, AddressId.CreateFor(_.SubaddressNr16Bus3Id), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid7Id, _.GebouwEenheid7Key, AddressId.CreateFor(_.SubaddressNr16Bus4Id), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumber15WithNewRelationRetired()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling3Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(3))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithHouseNumberId(_.HuisNr15Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSwitchHouseNumber15ToHouseNumber17())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid8Id, _.GebouwEenheid8Key, _.Address15Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid8Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address15Id, _.GebouwEenheid8Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder DeleteRelationWithHouseNr17()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNrKoppeling1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(4))))
                .WithHouseNumberId(_.HuisNr17Id)
                .WithModification(CrabModification.Delete);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumber15WithNewRelationRetired())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid6Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address17Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid7Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());

        }

        [Fact]
        public void AddHouseNumber15Test()
        {
            Assert(AddHouseNumber15());
        }

        [Fact]
        public void AddHouseNumber16Test()
        {
            Assert(AddHouseNumber16());
        }

        [Fact]
        public void AddSubaddress1Test()
        {
            Assert(AddSubaddress1WithFutureHouseNumber());
        }

        [Fact]
        public void AddSubaddress2Test()
        {
            Assert(AddSubaddress2WithFutureHouseNumber());
        }

        [Fact]
        public void AddSubaddress3Test()
        {
            Assert(AddSubaddress3WithFutureHouseNumber());
        }

        [Fact]
        public void AddSubaddress4Test()
        {
            Assert(AddSubaddress4WithFutureHouseNumber());
        }

        [Fact]
        public void SwitchHouseNr15To17Test()
        {
            Assert(AddSwitchHouseNumber15ToHouseNumber17());
        }

        [Fact]
        public void AddHouseNr15WithNewRelationRetiredTest()
        {
            Assert(AddHouseNumber15WithNewRelationRetired());
        }

        [Fact]
        public void DeleteRelationWithHouseNr17Test()
        {
            Assert(DeleteRelationWithHouseNr17());
        }
    }

}

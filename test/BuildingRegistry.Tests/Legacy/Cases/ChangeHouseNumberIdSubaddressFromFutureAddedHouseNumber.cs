namespace BuildingRegistry.Tests.Legacy.Cases
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
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class ChangeHouseNumberIdSubaddressFromFutureAddedHouseNumber : AutofacBasedTest
    {
        protected class TestCaseData
        {
            private const string BuildingWkt = "POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))";
            private const string HousePositionWkt = "POINT (14 20)";
            private const string NewHousePositionWkt = "POINT (20 20)";
            private const string Sub1PositionWkt = "POINT (21 36)";
            private const string Sub2PositionWkt = "POINT (32 32)";

            public TestCaseData(IFixture customizedFixture)
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
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }
            public CrabTerrainObjectHouseNumberId HuisNr18KoppelingId { get; }
            public CrabHouseNumberId HuisNr18Id { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

            public BuildingUnitKey GebouwEenheid0Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId);

            public BuildingUnitKey GebouwEenheid1Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);

            public BuildingUnitKey GebouwEenheid2Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid5Key => BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitId GebouwEenheid0Id => BuildingUnitId.Create(GebouwEenheid0Key, 1);
            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 1);
            public AddressId Address18Id => AddressId.CreateFor(HuisNr18Id);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address18Bus1Id => AddressId.CreateFor(SubaddressNr16Bus1Id);
            public AddressId Address18Bus2Id => AddressId.CreateFor(SubaddressNr16Bus2Id);
        }

        public ChangeHouseNumberIdSubaddressFromFutureAddedHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCaseData(Fixture);
        }

        protected readonly IFixture Fixture;
        protected TestCaseData _ { get; }


        public IEventCentricTestSpecificationBuilder ImportSubaddressWithOldHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.UtcNow.AddDays(-1))))
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressWithNewHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.UtcNow)))
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportSubaddressWithOldHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportNewHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportSubaddressWithNewHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid0Id, _.GebouwEenheid0Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address18Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportOtherSubaddressWithNewHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportNewHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address18Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid0Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address18Id, _.GebouwEenheid0Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address18Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportOldHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportOtherSubaddressWithNewHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void ImportSubaddressWithOldHouseNumberTest()
        {
            Assert(ImportSubaddressWithOldHouseNumber());
        }

        [Fact]
        public void ImportSubaddressWithNewHouseNumberTest()
        {
            Assert(ImportSubaddressWithNewHouseNumber());
        }

        [Fact]
        public void ImportNewHouseNumberTest()
        {
            Assert(ImportNewHouseNumber());
        }

        [Fact]
        public void ImportOtherSubaddressWithNewHouseNumberTest()
        {
            Assert(ImportOtherSubaddressWithNewHouseNumber());
        }

        [Fact]
        public void ImportOldHouseNumberTest()
        {
            Assert(ImportOldHouseNumber());
        }
    }
}

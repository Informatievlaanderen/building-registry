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
    using NetTopologySuite.IO;
    using NodaTime;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabSubaddressStatus;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class ChangeHouseNumberOfSubaddressIdThenOfHouseNumber : AutofacBasedTest
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
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus1Id);

            public BuildingUnitKey GebouwEenheid3Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

            public BuildingUnitKey GebouwEenheid4Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus2Id);

            public BuildingUnitKey GebouwEenheid5Key => GebouwEenheid1Key;

            public BuildingUnitId GebouwEenheid0Id => BuildingUnitId.Create(GebouwEenheid0Key, 1);
            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid1IdV2 => BuildingUnitId.Create(GebouwEenheid1Key, 2);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid2IdV2 => BuildingUnitId.Create(GebouwEenheid2Key, 2);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid3IdV2 => BuildingUnitId.Create(GebouwEenheid3Key, 2);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public AddressId Address18Id => AddressId.CreateFor(HuisNr18Id);
            public AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);
            public AddressId Address16Bus1Id => AddressId.CreateFor(SubaddressNr16Bus1Id);
            public AddressId Address16Bus2Id => AddressId.CreateFor(SubaddressNr16Bus2Id);
            public WkbGeometry BuildingGeometry => new WkbGeometry(new WKTReader().Read(BuildingWkt).AsBinary());
            public WkbGeometry CenterBuilding => new WkbGeometry(new WKTReader().Read(BuildingWkt).Centroid.AsBinary());
            public WkbGeometry HouseNrGeometry => new WkbGeometry(new WKTReader().Read(HousePositionWkt).AsBinary());
            public WkbGeometry NewHouseNrGeometry => new WkbGeometry(new WKTReader().Read(NewHousePositionWkt).AsBinary());
            public WkbGeometry Subaddr1Geometry => new WkbGeometry(new WKTReader().Read(Sub1PositionWkt).AsBinary());
            public WkbGeometry Subaddr2Geometry => new WkbGeometry(new WKTReader().Read(Sub2PositionWkt).AsBinary());
        }

        public ChangeHouseNumberOfSubaddressIdThenOfHouseNumber(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
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

        public IEventCentricTestSpecificationBuilder ImportOldHouseNumber()
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

        public IEventCentricTestSpecificationBuilder ImportSubaddressOnOldHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportOldHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressStatus()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithStatus(CrabAddressStatus.InUse)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportSubaddressOnOldHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressOnNewHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1))))
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithHouseNumberId(_.HuisNr18Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(ImportSubaddressStatus())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }


        public IEventCentricTestSpecificationBuilder ImportNewHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id); //koppel huisnr 18

            return new AutoFixtureScenario(Fixture)
                .Given(ImportSubaddressOnNewHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid2IdV2),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }


        [Fact]
        public void TestImportOldHouseNumber()
        {
            Assert(ImportOldHouseNumber());
        }

        [Fact]
        public void TestImportOldHouseNumberSubaddress()
        {
            Assert(ImportSubaddressOnOldHouseNumber());
        }

        [Fact]
        public void TestImportSubaddressStatus()
        {
            Assert(ImportSubaddressStatus());
        }

        [Fact]
        public void TestImportSubaddressOnNewHouseNumber()
        {
            Assert(ImportSubaddressOnNewHouseNumber());
        }

        [Fact]
        public void TestImportNewHouseNumber()
        {
            Assert(ImportNewHouseNumber());
        }
    }
}

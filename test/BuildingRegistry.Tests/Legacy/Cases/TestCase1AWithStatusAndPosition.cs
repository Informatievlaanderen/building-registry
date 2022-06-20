namespace BuildingRegistry.Tests.Legacy.Cases
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Events;
    using Fixtures;
    using NodaTime;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabHouseNumberPosition;
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObject;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase1AWithStatusAndPosition : AutofacBasedTest
    {
        public TestCase1AWithStatusAndPosition(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                SubaddressNr16Bus3Id = new CrabSubaddressId(163);
            }

            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
            public CrabSubaddressId SubaddressNr16Bus1Id { get; }
            public CrabSubaddressId SubaddressNr16Bus2Id { get; }
            public CrabSubaddressId SubaddressNr16Bus3Id { get; }

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

            public BuildingUnitKey GebouwEenheid6Key =>
                BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, SubaddressNr16Bus3Id);

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
            public AddressId Address16Bus3Id => AddressId.CreateFor(SubaddressNr16Bus3Id);
        }

        protected readonly IFixture Fixture;
        protected TestCase1AData _ { get; }

        public IEventCentricTestSpecificationBuilder T0()
        {
            var importGeometry = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(importGeometry)
                .Then(_.Gebouw1Id,
                    new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.AsBinary())),
                    importGeometry.ToLegacyEvent());

        }

        public IEventCentricTestSpecificationBuilder T1_PositionBeforeT1()
        {
            var importHouseNumberPositionFromCrab = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectId(_.Gebouw1CrabTerrainObjectId)
                .WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding)
                .WithTerrainObjectHouseNumber(_.HuisNr16KoppelingId)
                .WithPosition(new WkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given(T0())
                .When(importHouseNumberPositionFromCrab)
                .Then(_.Gebouw1Id,
                    importHouseNumberPositionFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1_StatusBeforeT1()
        {
            var importHouseNumberStatusFromCrab = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.InUse)
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T1_PositionBeforeT1())
                .When(importHouseNumberStatusFromCrab)
                .Then(_.Gebouw1Id,
                    importHouseNumberStatusFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T1_StatusBeforeT1())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPointInPolygon.AsBinary())),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid1Id),
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
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid2Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.Centroid.AsBinary())),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid3Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.Centroid.AsBinary())),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid3Id),
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
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid4Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.Centroid.AsBinary())),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.Address16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid5Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPointInPolygon.AsBinary())),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid5Id),
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
                .WithHouseNumberId(_.HuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T5())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid5Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T7()
        {
            var importTerrainObjectFromCrab = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T6())
                .When(importTerrainObjectFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasNotRealized(_.Gebouw1Id, new BuildingUnitId[0], new BuildingUnitId[0]),
                    importTerrainObjectFromCrab.ToLegacyEvent());
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
        public void TestT7()
        {
            Assert(T7());
        }
    }
}


namespace BuildingRegistry.Tests.Cases
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using NetTopologySuite.IO;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabHouseNumberPosition;
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabSubaddressPosition;
    using WhenImportingCrabTerrainObject;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase1B : AutofacBasedTest
    {
        private readonly IFixture _fixture;
        private TestCase1BData _ { get; }

        public TestCase1B(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture()
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16))
                ;

            _ = new TestCase1BData(_fixture);
        }

        public class TestCase1BData
        {
            private const string BuildingWkt = "POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))";
            private const string HousePositionWkt = "POINT (14 20)";
            private const string NewHousePositionWkt = "POINT (20 20)";
            private const string Sub1PositionWkt = "POINT (21 36)";
            private const string Sub2PositionWkt = "POINT (32 32)";

            public TestCase1BData(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
                HuisNr16KoppelingId = customizedFixture.Create<CrabTerrainObjectHouseNumberId>();
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                SubaddressNr16Bus1Id = new CrabSubaddressId(161);
                SubaddressNr16Bus2Id = new CrabSubaddressId(162);
            }

            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
            public CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
            public CrabHouseNumberId HuisNr16Id { get; }
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

            public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
            public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
            public BuildingUnitId GebouwEenheid4IdV2 => BuildingUnitId.Create(GebouwEenheid4Key, 2);
            public BuildingUnitId GebouwEenheid5Id => BuildingUnitId.Create(GebouwEenheid5Key, 2);
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

        public IEventCentricTestSpecificationBuilder T0()
        {
            var buildingGeometryFromCrab = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(_.BuildingGeometry);

            return new AutoFixtureScenario(_fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(buildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasOutlined(_.Gebouw1Id, GeometryHelper.CreateEwkbFrom(_.BuildingGeometry)),
                    buildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1Relation()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(_fixture)
                .Given(T0())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid1Id, GeometryHelper.CreateEwkbFrom(_.CenterBuilding)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1Geometry()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithPosition(_.HouseNrGeometry);

            return new AutoFixtureScenario(_fixture)
                .Given(T1Relation())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(_.Gebouw1Id, _.GebouwEenheid1Id, GeometryHelper.CreateEwkbFrom(_.HouseNrGeometry)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T1Status()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithStatus(CrabAddressStatus.InUse);

            return new AutoFixtureScenario(_fixture)
                .Given(T1Geometry())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2Relation()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(_fixture)
                .Given(T1Status())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid2Id, GeometryHelper.CreateEwkbFrom(_.CenterBuilding)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid3Id, GeometryHelper.CreateEwkbFrom(_.CenterBuilding)),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2Geometry()
        {
            var importSubaddressPositionFromCrab = _fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithPosition(_.Subaddr1Geometry);

            return new AutoFixtureScenario(_fixture)
                .Given(T2Relation())
                .When(importSubaddressPositionFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(_.Gebouw1Id, _.GebouwEenheid2Id, GeometryHelper.CreateEwkbFrom(_.Subaddr1Geometry)),
                    importSubaddressPositionFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T2Status()
        {
            var importSubaddressStatusFromCrab = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.Proposed);

            return new AutoFixtureScenario(_fixture)
                .Given(T2Geometry())
                .When(importSubaddressStatusFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid2Id),
                    importSubaddressStatusFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3Relation()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(_fixture)
                .Given(T2Status())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid4Id, GeometryHelper.CreateEwkbFrom(_.CenterBuilding)),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3Geometry()
        {
            var importSubaddressPositionFromCrab = _fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithPosition(_.Subaddr2Geometry);

            return new AutoFixtureScenario(_fixture)
                .Given(T3Relation())
                .When(importSubaddressPositionFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(_.Gebouw1Id, _.GebouwEenheid4Id, GeometryHelper.CreateEwkbFrom(_.Subaddr2Geometry)),
                    importSubaddressPositionFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3Status()
        {
            var importSubaddressStatusFromCrab = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithStatus(CrabAddressStatus.InUse);

            return new AutoFixtureScenario(_fixture)
                .Given(T3Geometry())
                .When(importSubaddressStatusFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid4Id),
                    importSubaddressStatusFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T4()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
                .Given(T3Status())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.Address16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitPositionWasAppointedByAdministrator(_.Gebouw1Id, _.GebouwEenheid5Id, GeometryHelper.CreateEwkbFrom(_.HouseNrGeometry)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid5Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T5()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
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
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
                .Given(T5())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid5Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T7()
        {
            var importTerrainObjectFromCrab = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
                .Given(T6())
                .When(importTerrainObjectFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasNotRealized(_.Gebouw1Id, new BuildingUnitId[0], new BuildingUnitId[0]),
                    importTerrainObjectFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void TestT1Relation()
        {
            Assert(T1Relation());
        }

        [Fact]
        public void TestT1Geometry()
        {
            Assert(T1Geometry());
        }

        [Fact]
        public void TestCompeleteT1()
        {
            Assert(T1Status());
        }

        [Fact]
        public void TestT2Relation()
        {
            Assert(T2Relation());
        }

        [Fact]
        public void TestT2Geometry()
        {
            Assert(T2Geometry());
        }

        [Fact]
        public void TestCompleteT2()
        {
            Assert(T2Status());
        }

        [Fact]
        public void TestT3Relation()
        {
            Assert(T3Relation());
        }

        [Fact]
        public void TestT3Geometry()
        {
            Assert(T3Geometry());
        }

        [Fact]
        public void TestCompleteT3()
        {
            Assert(T3Status());
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

        public IEventCentricTestSpecificationBuilder BasedOnT3AddHousenrStatusRecord()
        {
            var importHouseNumberStatusFromCrab = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Proposed);

            return new AutoFixtureScenario(_fixture)
                .Given(T3Status())
                .When(importHouseNumberStatusFromCrab)
                .Then(_.Gebouw1Id,
                    importHouseNumberStatusFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3StatusThenGoToT4()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT3AddHousenrStatusRecord())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.Address16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitPositionWasAppointedByAdministrator(_.Gebouw1Id, _.GebouwEenheid5Id, GeometryHelper.CreateEwkbFrom(_.HouseNrGeometry)),
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid5Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3PositionThenGoToT4()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT3AddHousenrPositionRecord())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasReaddedByOtherUnitRemoval(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, _.Address16Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid5Id, GeometryHelper.CreateEwkbFrom(_.NewHouseNrGeometry)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid5Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3AddedNewUnitHasStatusByImportingSameStatusShouldNotChange()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithStatus(CrabAddressStatus.Proposed);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT3StatusThenGoToT4())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }


        [Fact]
        public void BasedOnT3AddHousenrStatusThenGoToT4AndAddedUnitHasNewStatus()
        {
            Assert(BasedOnT3AddedNewUnitHasStatusByImportingSameStatusShouldNotChange());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3AddHousenrPositionRecord()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding)
                .WithPosition(_.NewHouseNrGeometry);

            return new AutoFixtureScenario(_fixture)
                .Given(T3Status())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3AddedNewUnitHasPositionByImportingSameStatusShouldNotChange()
        {
            var importTerrainObjectHouseNumberFromCrab = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding)
                .WithPosition(_.NewHouseNrGeometry);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT3PositionThenGoToT4())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3AddPositionThenGoToT4AndAddedUnitHasNewPosition()
        {
            Assert(BasedOnT3AddedNewUnitHasPositionByImportingSameStatusShouldNotChange());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4AddSubaddressStatus()
        {
            var importSubaddressStatusFromCrab = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithStatus(CrabAddressStatus.Proposed);

            return new AutoFixtureScenario(_fixture)
                .Given(T4())
                .When(importSubaddressStatusFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressStatusFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4ReaddSubaddress2WithNewStatus()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT4AddSubaddressStatus())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid4Id),
                    new BuildingUnitPositionWasAppointedByAdministrator(_.Gebouw1Id, _.GebouwEenheid4IdV2, GeometryHelper.CreateEwkbFrom(_.Subaddr2Geometry)),
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid4IdV2),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid4IdV2),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4AddSubaddressStatusToNewAddedUnit()
        {
            var importSubaddressStatusFromCrab = _fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithStatus(CrabAddressStatus.Proposed);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT4ReaddSubaddress2WithNewStatus())
                .When(importSubaddressStatusFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressStatusFromCrab.ToLegacyEvent());
        }


        [Fact]
        public void BasedOnT4AddSubaddressStatusThenReaddSubaddressAndAlreadyHasNewStatus()
        {
            Assert(BasedOnT4AddSubaddressStatusToNewAddedUnit());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4RetireSubaddressPosition()
        {
            var importSubaddressPositionFromCrab = _fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), LocalDateTime.FromDateTime(DateTime.Now.AddDays(1))))
                .WithPosition(_.Subaddr2Geometry);

            return new AutoFixtureScenario(_fixture)
                .Given(T4())
                .When(importSubaddressPositionFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressPositionFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4AddSubaddressPosition()
        {
            var importSubaddressPositionFromCrab = _fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPosition(_.NewHouseNrGeometry)
                .WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT4RetireSubaddressPosition())
                .When(importSubaddressPositionFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressPositionFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4ReaddSubaddress2WithNewPosition()
        {
            var importSubaddressFromCrab = _fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT4AddSubaddressPosition())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid4Id),
                    new BuildingUnitPositionWasDerivedFromObject(_.Gebouw1Id, _.GebouwEenheid4IdV2, GeometryHelper.CreateEwkbFrom(_.NewHouseNrGeometry)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid4IdV2),
                    new BuildingUnitBecameComplete(_.Gebouw1Id, _.GebouwEenheid4IdV2),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid5Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT4AddSubaddressPositionToNewAddedUnit()
        {
            var importSubaddressPositionFromCrab = _fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(2)), null))
                .WithPosition(_.NewHouseNrGeometry)
                .WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding);

            return new AutoFixtureScenario(_fixture)
                .Given(BasedOnT4ReaddSubaddress2WithNewPosition())
                .When(importSubaddressPositionFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressPositionFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT4AddSubaddressPositionThenReaddSubaddressAndAlreadyHasNewPosition()
        {
            Assert(BasedOnT4AddSubaddressPositionToNewAddedUnit());
        }
    }
}

namespace BuildingRegistry.Tests.WhenReaddressingSubaddress
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using NetTopologySuite.Geometries;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabSubaddressPosition;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPositionAfterReaddressSubaddress : AutofacBasedTest
    {
        protected readonly IFixture Fixture;
        private readonly Point _position;
        private readonly Geometry _geometry;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
        private CrabHouseNumberId HuisNr16Id { get; }
        public CrabSubaddressId OldSubaddressNr16Bus1Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);
        public BuildingUnitKey OldGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, OldSubaddressNr16Bus1Id);
        public BuildingUnitKey NewGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, NewSubaddressNr16Bus1Id);
        public BuildingUnitKey GebouwEenheid3Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(OldGebouwEenheid2Key, 1);
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);

        private AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);

        public AddressId OldAddress16Bus1Id => AddressId.CreateFor(OldSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);

        public GivenPositionAfterReaddressSubaddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithValidPolygon())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            HuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            HuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            OldSubaddressNr16Bus1Id = new CrabSubaddressId(161);
            NewSubaddressNr16Bus1Id = new CrabSubaddressId(163);
            ReaddressingBeginDate = new ReaddressingBeginDate(Fixture.Create<LocalDate>());

            _geometry = new WKBReader().Read(Fixture.Create<WkbGeometry>());
            _position = _geometry.Centroid;
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddBuildingPosition()
        {
            var importBuildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithGeometry(new WkbGeometry(_geometry.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importBuildingGeometryFromCrab)
                .Then(Gebouw1Id,
                    new BuildingWasMeasuredByGrb(Gebouw1Id, GeometryHelper.CreateEwkbFrom(Fixture.Create<WkbGeometry>())),
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid1Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    importBuildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddBuildingPosition())
                .When(importSubaddress)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid2Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid3Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    new BuildingUnitBecameComplete(Gebouw1Id, GebouwEenheid3Id),
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSubaddressPosition()
        {
            var wkbGeometry = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var importSubaddressPosition = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithPosition(wkbGeometry)
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(importSubaddressPosition)
                .Then(Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(Gebouw1Id, GebouwEenheid2Id, GeometryHelper.CreateEwkbFrom(wkbGeometry)),
                    importSubaddressPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            var importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(SetSubaddressPosition())
                .When(importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSubaddressPositionWithNewSubaddressId()
        {
            var wkbGeometry = new WkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary());
            var importSubaddressPosition = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromBuilding)
                .WithPosition(wkbGeometry)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(importSubaddressPosition)
                .Then(Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(Gebouw1Id, GebouwEenheid2Id, GeometryHelper.CreateEwkbFrom(wkbGeometry)),
                    importSubaddressPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetReaddressedSubaddressPositionWithOldSubaddressId()
        {
            var otherPoint = GeometryHelper.OtherValidPointInPolygon;

            var importPosition = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithPosition(new WkbGeometry(otherPoint.AsBinary()))
                .WithSubaddressId(OldSubaddressNr16Bus1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(SetSubaddressPositionWithNewSubaddressId())
                .When(importPosition)
                .Then(Gebouw1Id,
                    importPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSamePositionWithNewSubaddressIdButBeforePositionOldSubaddress()
        {
            var wkbGeometry = new WkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary());
            var importSubaddressPosition = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromBuilding)
                .WithPosition(wkbGeometry)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(SetReaddressedSubaddressPositionWithOldSubaddressId())
                .When(importSubaddressPosition)
                .Then(Gebouw1Id,
                    importSubaddressPosition.ToLegacyEvent());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddBuildingPositionTest()
        {
            Assert(AddBuildingPosition());
        }

        [Fact]
        public void AddSubaddressUnitTest()
        {
            Assert(AddSubaddress());
        }

        [Fact]
        public void SetPositionTest()
        {
            Assert(SetSubaddressPosition());
        }

        [Fact]
        public void AddReaddressingSubaddressTest()
        {
            Assert(AddReaddressingOfSubaddress());
        }

        [Fact]
        public void SetPositionWithNewSubaddressIdTest()
        {
            Assert(SetSubaddressPositionWithNewSubaddressId());
        }

        [Fact]
        public void SetReaddressedSubaddressPositionWithOldSubaddressIdTest()
        {
            Assert(SetReaddressedSubaddressPositionWithOldSubaddressId());
        }

        [Fact]
        public void SetSamePositionWithNewSubaddressIdButBeforePositionOldSubaddressTest()
        {
            Assert(SetSamePositionWithNewSubaddressIdButBeforePositionOldSubaddress());
        }
    }
}

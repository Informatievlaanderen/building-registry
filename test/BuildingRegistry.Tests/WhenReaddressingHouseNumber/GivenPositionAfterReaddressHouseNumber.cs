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
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabHouseNumberPosition;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPositionAfterReaddressHouseNumber : AutofacBasedTest
    {
        protected readonly IFixture Fixture;
        private readonly Point _position;
        private readonly Geometry _geometry;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId OldHuisNr16KoppelingId { get; }
        private CrabHouseNumberId OldHuisNr16Id { get; }

        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);
        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, OldHuisNr16KoppelingId);
        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
        private ReaddressingBeginDate ReaddressingBeginDate { get; }

        public GivenPositionAfterReaddressHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(16);
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
            OldHuisNr16Id = new CrabHouseNumberId(161616);
            NewHuisNr16Id = new CrabHouseNumberId(171717);
            ReaddressingBeginDate = new ReaddressingBeginDate(Fixture.Create<LocalDate>());

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());

            _geometry = new WKBReader().Read(polygonFixture.Create<WkbGeometry>());
            _position = _geometry.Centroid;
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
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
                    new BuildingWasMeasuredByGrb(Gebouw1Id, new ExtendedWkbGeometry(_geometry.AsBinary())),
                    importBuildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetHouseNumberPosition()
        {
            var importPosition = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(OldHuisNr16KoppelingId)
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding)
                .WithPosition(new WkbGeometry(_position.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given(AddBuildingPosition())
                .When(importPosition)
                .Then(Gebouw1Id,
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid1Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    importPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(SetHouseNumberPosition())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressingBeginDate),
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetPositionWithNewHouseNumberId()
        {
            var importPosition = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(NewHuisNr16KoppelingId)
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithPosition(new WkbGeometry(_position.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importPosition)
                .Then(Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(Gebouw1Id, GebouwEenheid1Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    importPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetReaddressedHouseNrPositionWithOldHouseNumberId()
        {
            var otherPoint = GeometryHelper.ValidPointInPolygon;

            var importPosition = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(OldHuisNr16KoppelingId)
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithPosition(new WkbGeometry(otherPoint.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given(SetPositionWithNewHouseNumberId())
                .When(importPosition)
                .Then(Gebouw1Id,
                    importPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSamePositionWithNewHouseNumberIdButBeforePositionOldHouseNr()
        {
            var importPosition = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(NewHuisNr16KoppelingId)
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithPosition(new WkbGeometry(_position.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given(SetReaddressedHouseNrPositionWithOldHouseNumberId())
                .When(importPosition)
                .Then(Gebouw1Id,
                    importPosition.ToLegacyEvent());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void SetHouseNumberPositionTest()
        {
            Assert(SetHouseNumberPosition());
        }

        [Fact]
        public void AddReaddressingHouseNumberTest()
        {
            Assert(AddReaddressingOfHouseNumber());
        }

        [Fact]
        public void AddReaddressedHouseNumberPositionWithNewHouseNumberIdTest()
        {
            Assert(SetPositionWithNewHouseNumberId());
        }

        [Fact]
        public void AddReaddressedOldHouseNumberPositionWithOldHouseNumberTest()
        {
            Assert(SetReaddressedHouseNrPositionWithOldHouseNumberId());
        }

        [Fact]
        public void SetSamePositionWithNewHouseNumberIdButBeforePositionOldHouseNrTest()
        {
            Assert(SetSamePositionWithNewHouseNumberIdButBeforePositionOldHouseNr());
        }
    }
}

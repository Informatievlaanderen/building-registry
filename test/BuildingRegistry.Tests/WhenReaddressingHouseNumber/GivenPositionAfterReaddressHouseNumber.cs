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
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.DataStructures;
    using Building.Events.Crab;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabHouseNumberPosition;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPositionAfterReaddressHouseNumber : SnapshotBasedTest
    {
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

        private BuildingUnitWasAdded buildingUnitWasAdded;
        private BuildingUnitWasReaddressed buildingUnitWasReaddressed;
        private ImportReaddressingHouseNumberFromCrab importReaddressingHouseNumberFromCrab;
        private ImportBuildingGeometryFromCrab importBuildingGeometryFromCrab;
        private ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab1;
        private ImportHouseNumberPositionFromCrab importHouseNumberPositionFromCrab2;

        public GivenPositionAfterReaddressHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
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

            buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key,
                OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    buildingUnitWasAdded,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddBuildingPosition()
        {
            importBuildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithGeometry(new WkbGeometry(_geometry.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            var buildingWasMeasuredByGrb =
                new BuildingWasMeasuredByGrb(Gebouw1Id, new ExtendedWkbGeometry(_geometry.AsBinary()));
            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importBuildingGeometryFromCrab)
                .Then(Gebouw1Id,
                    buildingWasMeasuredByGrb,
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

            importHouseNumberPositionFromCrab1 = importPosition;

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

            buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id,
                NewAddress16Id, ReaddressingBeginDate);
            importReaddressingHouseNumberFromCrab = importReaddressingHouseNumber;

            return new AutoFixtureScenario(Fixture)
                .Given(SetHouseNumberPosition())
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    buildingUnitWasReaddressed,
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

            importHouseNumberPositionFromCrab2 = importPosition;

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
            Fixture.Customize(new WithSnapshotInterval(1));

            var importPosition = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(NewHuisNr16KoppelingId)
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithPosition(new WkbGeometry(_position.AsBinary()));

            return new AutoFixtureScenario(Fixture)
                .Given(SetReaddressedHouseNrPositionWithOldHouseNumberId())
                .When(importPosition)
                .Then(new[]
                {
                    new Fact(Gebouw1Id, importPosition.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(Gebouw1Id),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(Gebouw1Id)
                            .WithGeometry(new BuildingGeometry(new ExtendedWkbGeometry(_geometry.AsBinary()), BuildingGeometryMethod.MeasuredByGrb))
                            .WithGeometryChronicle(new List<ImportBuildingGeometryFromCrab>()
                            {
                                importBuildingGeometryFromCrab
                            })
                            .WithLastModificationFromCrab(Modification.Update)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                                {
                                    {
                                        importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId,
                                        importReaddressingHouseNumberFromCrab.NewHouseNumberId
                                    },
                                })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                                {
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId
                                }
                            )
                            .WithHouseNumberReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>()
                            {
                                {BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                    importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId), importReaddressingHouseNumberFromCrab.ToLegacyEvent()}
                            })
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)

                                            .WithAddressIds(new []
                                            {
                                                new AddressId(buildingUnitWasReaddressed.NewAddressId)
                                            })
                                            .WithReaddressedEvents(new List<BuildingUnitWasReaddressed>()
                                            {
                                                buildingUnitWasReaddressed,
                                            })
                                            .WithPosition(new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(_position.AsBinary()), BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                                            .WithHouseNumberPositions(new List<AddressHouseNumberPositionWasImportedFromCrab>
                                            {
                                                importHouseNumberPositionFromCrab1.ToLegacyEvent(),
                                                importHouseNumberPositionFromCrab2.ToLegacyEvent(),
                                                importPosition.ToLegacyEvent()
                                            })
                                    })
                                    .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>()
                                    {
                                        {
                                            BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                                importReaddressingHouseNumberFromCrab.NewTerrainObjectHouseNumberId),
                                            BuildingUnitKey.Create(importReaddressingHouseNumberFromCrab.TerrainObjectId,
                                                importReaddressingHouseNumberFromCrab.OldTerrainObjectHouseNumberId)
                                        }
                                    })
                            )
                            .WithHouseNumberPositionEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>
                            {
                                { AddressId.CreateFor(importPosition.HouseNumberId), new List<AddressHouseNumberPositionWasImportedFromCrab>
                                    {
                                        importHouseNumberPositionFromCrab1.ToLegacyEvent(),
                                        importHouseNumberPositionFromCrab2.ToLegacyEvent(),
                                        importPosition.ToLegacyEvent()
                                    }
                                }
                            })
                            .Build(12, EventSerializerSettings))
                });
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

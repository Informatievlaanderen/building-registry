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
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.DataStructures;
    using Building.Events.Crab;
    using NetTopologySuite.Geometries;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabSubaddressPosition;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenPositionAfterReaddressSubaddress : SnapshotBasedTest
    {
        private readonly Point _position;
        private readonly Geometry _geometry;
        private WkbGeometry _wkbGeometry;

        #region Snapshot variables

        private BuildingUnitWasAdded? _buildingUnitWasAdded;
        private ImportBuildingGeometryFromCrab? _importBuildingGeometryFromCrab;
        private ImportSubaddressFromCrab? _importSubaddress;
        private ImportSubaddressPositionFromCrab? _importSubaddressPosition;
        private ImportReaddressingSubaddressFromCrab? _importReaddressingSubaddress;
        private ImportSubaddressPositionFromCrab? _importNewSubaddressPosition;
        private BuildingUnitWasAdded? _buildingUnit2WasAdded;
        private CommonBuildingUnitWasAdded? _commonBuildingUnitWasAdded;
        private BuildingUnitWasReaddressed? _buildingUnitWasReaddressed;

        #endregion

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
            Fixture
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

            _wkbGeometry = Fixture.Create<WkbGeometry>();
            _geometry = new WKBReader().Read(_wkbGeometry);
            _position = _geometry.Centroid;
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id);

            _buildingUnitWasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    _buildingUnitWasAdded,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddBuildingPosition()
        {
            _importBuildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithTerrainObjectId(Gebouw1CrabTerrainObjectId)
                .WithGeometry(new WkbGeometry(_geometry.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(_importBuildingGeometryFromCrab)
                .Then(Gebouw1Id,
                    new BuildingWasMeasuredByGrb(Gebouw1Id, GeometryHelper.CreateEwkbFrom(_wkbGeometry)),
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid1Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    _importBuildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            _importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            _buildingUnit2WasAdded = new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(_importSubaddress.Timestamp));
            _commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(_importSubaddress.Timestamp));
            return new AutoFixtureScenario(Fixture)
                .Given(AddBuildingPosition())
                .When(_importSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnit2WasAdded,
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid2Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    _commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    new BuildingUnitPositionWasDerivedFromObject(Gebouw1Id, GebouwEenheid3Id, ExtendedWkbGeometry.CreateEWkb(_position.AsBinary())),
                    new BuildingUnitBecameComplete(Gebouw1Id, GebouwEenheid3Id),
                    _importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSubaddressPosition()
        {
            var wkbGeometry = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            _importSubaddressPosition = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithPosition(wkbGeometry)
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(_importSubaddressPosition)
                .Then(Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(Gebouw1Id, GebouwEenheid2Id, GeometryHelper.CreateEwkbFrom(wkbGeometry)),
                    _importSubaddressPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            _importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            _buildingUnitWasReaddressed = new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate);
            return new AutoFixtureScenario(Fixture)
                .Given(SetSubaddressPosition())
                .When(_importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    _buildingUnitWasReaddressed,
                    _importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSubaddressPositionWithNewSubaddressId()
        {
            var wkbGeometry = new WkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary());
            _importNewSubaddressPosition = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromBuilding)
                .WithPosition(wkbGeometry)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(_importNewSubaddressPosition)
                .Then(Gebouw1Id,
                    new BuildingUnitPositionWasAppointedByAdministrator(Gebouw1Id, GebouwEenheid2Id, GeometryHelper.CreateEwkbFrom(wkbGeometry)),
                    _importNewSubaddressPosition.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetReaddressedSubaddressPositionWithOldSubaddressId()
        {
            var otherPoint = GeometryHelper.OtherValidPointInPolygon;

            var importPositionOldSubaddress = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(LocalDateTime.FromDateTime(DateTime.Now.AddDays(1)), null))
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)
                .WithPosition(new WkbGeometry(otherPoint.AsBinary()))
                .WithSubaddressId(OldSubaddressNr16Bus1Id);

            return new AutoFixtureScenario(Fixture)
                .Given(SetSubaddressPositionWithNewSubaddressId())
                .When(importPositionOldSubaddress)
                .Then(Gebouw1Id,
                    importPositionOldSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder SetSamePositionWithNewSubaddressIdButBeforePositionOldSubaddress()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

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
                .Then(new Fact[]
                    {
                        new Fact(Gebouw1Id, importSubaddressPosition.ToLegacyEvent()),
                        new Fact(GetSnapshotIdentifier(Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(Gebouw1Id)
                            .WithGeometry(new BuildingGeometry(ExtendedWkbGeometry.CreateEWkb(_wkbGeometry), BuildingGeometryMethod.MeasuredByGrb))
                            .WithGeometryChronicle(_importBuildingGeometryFromCrab)
                            .WithLastModificationFromCrab(Modification.Update)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>{{HuisNr16KoppelingId, HuisNr16Id}})
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{HuisNr16KoppelingId})
                            .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                            {
                                { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(HuisNr16KoppelingId, HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>{ _importSubaddress.ToLegacyEvent() } }
                            })
                            .WithSubaddressPositionEventsBySubaddressId(new Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>>
                            {
                                { OldSubaddressNr16Bus1Id, new List<AddressSubaddressPositionWasImportedFromCrab> { _importSubaddressPosition.ToLegacyEvent() } },
                                { NewSubaddressNr16Bus1Id, new List<AddressSubaddressPositionWasImportedFromCrab> { _importNewSubaddressPosition.ToLegacyEvent(), importSubaddressPosition.ToLegacyEvent() } }
                            })
                            .WithSubaddressReaddressedEventsByBuildingUnit(new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>
                            {
                                { OldGebouwEenheid2Key, _importReaddressingSubaddress.ToLegacyEvent()  }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded)
                                        .WithPosition(new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(_position.AsBinary()), BuildingUnitPositionGeometryMethod.DerivedFromObject)),

                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                        .WithAddressIds(new List<AddressId>{NewAddress16Bus1Id})
                                        .WithPosition(new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPointInPolygon.AsBinary()), BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                                        .WithReaddressedEvents(_buildingUnitWasReaddressed)
                                        .WithSubaddressPositions(new List<AddressSubaddressPositionWasImportedFromCrab>{ _importSubaddressPosition.ToLegacyEvent(), _importNewSubaddressPosition.ToLegacyEvent(), importSubaddressPosition.ToLegacyEvent()}),

                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                        .BecameComplete(true)
                                        .WithStatus(BuildingUnitStatus.Realized)
                                        .WithPosition(new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(_position.AsBinary()), BuildingUnitPositionGeometryMethod.DerivedFromObject))
                                })
                                .WithReaddressedKeys(new Dictionary<BuildingUnitKey, BuildingUnitKey>
                                {
                                    { NewGebouwEenheid2Key, OldGebouwEenheid2Key }
                                }))
                            .Build(20, EventSerializerSettings))
                    });
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

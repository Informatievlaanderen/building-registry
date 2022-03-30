namespace BuildingRegistry.Tests.Cases
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building;
    using Building.Commands.Crab;
    using Building.Events;
    using Building.Events.Crab;
    using FluentAssertions;
    using NetTopologySuite.IO;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class AddRetiredHouseNumberAfterNonRetiredSubaddressWasBuffered : AutofacBasedTest
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
            public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
            public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);
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

        public AddRetiredHouseNumberAfterNonRetiredSubaddressWasBuffered(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCaseData(Fixture);
            _building = Building.Register(_.Gebouw1Id);
        }

        private Building _building;
        private ImportSubaddressFromCrab _importSubaddressFromCrab;
        protected IFixture Fixture { get; }
        protected TestCaseData _ { get; }

        //Import irrelevant houseNumber just to have one unit
        //Buffer subaddress with infinite lifetime
        //Import terrainObjectHousenr with finite lifetime
        //Expects: last 2 retired
        //Actual: adds active subaddress unit then retires => error when creating common unit

        [Fact]
        public int GivenAnotherHouseNumberWasImported()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id);

            _building.ImportTerrainObjectHouseNumberFromCrab(
                importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId,
                importTerrainObjectHouseNumberFromCrab.TerrainObjectId,
                importTerrainObjectHouseNumberFromCrab.HouseNumberId,
                importTerrainObjectHouseNumberFromCrab.Lifetime,
                importTerrainObjectHouseNumberFromCrab.Timestamp,
                importTerrainObjectHouseNumberFromCrab.Operator,
                importTerrainObjectHouseNumberFromCrab.Modification,
                importTerrainObjectHouseNumberFromCrab.Organisation);

            _building
                .GetChanges()
                .Skip(1) //registered
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid0Id, _.GebouwEenheid0Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        [Fact]
        public int BufferHouseNumberStatus()
        {
            var skip = GivenAnotherHouseNumberWasImported();

            var houseNumberStatusFromCrab = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.InUse);

            _building.ImportHouseNumberStatusFromCrab(houseNumberStatusFromCrab.TerrainObjectId,
                houseNumberStatusFromCrab.TerrainObjectHouseNumberId,
                houseNumberStatusFromCrab.HouseNumberStatusId,
                houseNumberStatusFromCrab.HouseNumberId,
                houseNumberStatusFromCrab.AddressStatus,
                houseNumberStatusFromCrab.Lifetime,
                houseNumberStatusFromCrab.Timestamp,
                houseNumberStatusFromCrab.Operator,
                houseNumberStatusFromCrab.Modification,
                houseNumberStatusFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    houseNumberStatusFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        [Fact]
        public int BufferSubaddressStatus()
        {
            var skip = BufferHouseNumberStatus();

            var importSubaddressStatusFromCrab = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.InUse);

            _building.ImportSubaddressStatusFromCrab(importSubaddressStatusFromCrab.TerrainObjectId,

                importSubaddressStatusFromCrab.TerrainObjectHouseNumberId,
                importSubaddressStatusFromCrab.SubaddressStatusId,
                importSubaddressStatusFromCrab.SubaddressId,
                importSubaddressStatusFromCrab.SubaddressStatus,
                importSubaddressStatusFromCrab.Lifetime,
                importSubaddressStatusFromCrab.Timestamp,
                importSubaddressStatusFromCrab.Operator,
                importSubaddressStatusFromCrab.Modification,
                importSubaddressStatusFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    importSubaddressStatusFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        [Fact]
        public int AddNonRetiredSubaddress()
        {
            var skip = BufferSubaddressStatus();

            _importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _building.ImportSubaddressFromCrab(_importSubaddressFromCrab.TerrainObjectId,
                _importSubaddressFromCrab.TerrainObjectHouseNumberId,
                _importSubaddressFromCrab.SubaddressId,
                _importSubaddressFromCrab.HouseNumberId,
                _importSubaddressFromCrab.BoxNumber,
                _importSubaddressFromCrab.BoxNumberType,
                _importSubaddressFromCrab.Lifetime,
                _importSubaddressFromCrab.Timestamp,
                _importSubaddressFromCrab.Operator,
                _importSubaddressFromCrab.Modification,
                _importSubaddressFromCrab.Organisation);

            var expected = _importSubaddressFromCrab.ToLegacyEvent();

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object> { expected });

            return _building.GetChanges().Count();
        }

        [Fact]
        public int AddRetiredHouseNumber()
        {
            var skip = AddNonRetiredSubaddress();

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            _building.ImportTerrainObjectHouseNumberFromCrab(
                importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId,
                importTerrainObjectHouseNumberFromCrab.TerrainObjectId,
                importTerrainObjectHouseNumberFromCrab.HouseNumberId,
                importTerrainObjectHouseNumberFromCrab.Lifetime,
                importTerrainObjectHouseNumberFromCrab.Timestamp,
                importTerrainObjectHouseNumberFromCrab.Operator,
                importTerrainObjectHouseNumberFromCrab.Modification,
                importTerrainObjectHouseNumberFromCrab.Organisation);

            var expected = new List<object>
            {
                new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id,
                    new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid1Id),
                new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id,
                    new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid2Id),
                new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus1Id, _.GebouwEenheid2Id),
                importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()
            };

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(expected);

            return _building.GetChanges().Count();
        }
    }
}

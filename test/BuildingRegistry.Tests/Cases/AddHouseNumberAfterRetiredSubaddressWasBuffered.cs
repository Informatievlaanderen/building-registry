namespace BuildingRegistry.Tests.Cases
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using FluentAssertions;
    using NetTopologySuite.IO;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class AddHouseNumberAfterRetiredSubaddressWasBuffered : SnapshotBasedTest
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

        public AddHouseNumberAfterRetiredSubaddressWasBuffered(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _ = new TestCaseData(Fixture);
            _building = Building.Register(_.Gebouw1Id, new BuildingFactory(IntervalStrategy.Default));
        }

        private Building _building;
        private ImportSubaddressFromCrab _importSubaddressFromCrab;
        private ImportSubaddressFromCrab _importSubaddress2FromCrab;
        private ImportSubaddressStatusFromCrab? _importSubaddress2StatusFromCrab;
        private ImportSubaddressStatusFromCrab? _importSubaddressStatusFromCrab;
        protected TestCaseData _ { get; }

        [Fact]
        public int BufferSubaddress1Status()
        {
            var skip = 1;

            _importSubaddressStatusFromCrab = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.InUse);

            _building.ImportSubaddressStatusFromCrab(_importSubaddressStatusFromCrab.TerrainObjectId,

                _importSubaddressStatusFromCrab.TerrainObjectHouseNumberId,
                _importSubaddressStatusFromCrab.SubaddressStatusId,
                _importSubaddressStatusFromCrab.SubaddressId,
                _importSubaddressStatusFromCrab.SubaddressStatus,
                _importSubaddressStatusFromCrab.Lifetime,
                _importSubaddressStatusFromCrab.Timestamp,
                _importSubaddressStatusFromCrab.Operator,
                _importSubaddressStatusFromCrab.Modification,
                _importSubaddressStatusFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _importSubaddressStatusFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        [Fact]
        public int BufferSubaddress2Status()
        {
            var skip = BufferSubaddress1Status();

            _importSubaddress2StatusFromCrab = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.Proposed);

            _building.ImportSubaddressStatusFromCrab(_importSubaddress2StatusFromCrab.TerrainObjectId,

                _importSubaddress2StatusFromCrab.TerrainObjectHouseNumberId,
                _importSubaddress2StatusFromCrab.SubaddressStatusId,
                _importSubaddress2StatusFromCrab.SubaddressId,
                _importSubaddress2StatusFromCrab.SubaddressStatus,
                _importSubaddress2StatusFromCrab.Lifetime,
                _importSubaddress2StatusFromCrab.Timestamp,
                _importSubaddress2StatusFromCrab.Operator,
                _importSubaddress2StatusFromCrab.Modification,
                _importSubaddress2StatusFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _importSubaddress2StatusFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        [Fact]
        public int AddNonRetiredSubaddress()
        {
            var skip = BufferSubaddress2Status();

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

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(_importSubaddressFromCrab.ToLegacyEvent());

            return _building.GetChanges().Count();
        }


        [Fact]
        public int AddRetiredSubaddress()
        {
            var skip = AddNonRetiredSubaddress();

            _importSubaddress2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _building.ImportSubaddressFromCrab(_importSubaddress2FromCrab.TerrainObjectId,
                _importSubaddress2FromCrab.TerrainObjectHouseNumberId,
                _importSubaddress2FromCrab.SubaddressId,
                _importSubaddress2FromCrab.HouseNumberId,
                _importSubaddress2FromCrab.BoxNumber,
                _importSubaddress2FromCrab.BoxNumberType,
                _importSubaddress2FromCrab.Lifetime,
                _importSubaddress2FromCrab.Timestamp,
                _importSubaddress2FromCrab.Operator,
                _importSubaddress2FromCrab.Modification,
                _importSubaddress2FromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(_importSubaddress2FromCrab.ToLegacyEvent());

            return _building.GetChanges().Count();
        }

        [Fact]
        public int AddHouseNumber()
        {
            var skip = AddRetiredSubaddress();

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _building.ImportTerrainObjectHouseNumberFromCrab(
                importTerrainObjectHouseNumberFromCrab.TerrainObjectHouseNumberId,
                importTerrainObjectHouseNumberFromCrab.TerrainObjectId,
                importTerrainObjectHouseNumberFromCrab.HouseNumberId,
                importTerrainObjectHouseNumberFromCrab.Lifetime,
                importTerrainObjectHouseNumberFromCrab.Timestamp,
                importTerrainObjectHouseNumberFromCrab.Operator,
                importTerrainObjectHouseNumberFromCrab.Modification,
                importTerrainObjectHouseNumberFromCrab.Organisation);

            var buildingUnitWasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            var buildingUnit2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            var buildingUnit3WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    buildingUnitWasAdded,
                    buildingUnit2WasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    buildingUnit3WasAdded,
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()
                });

            _building.TakeSnapshot()
                .Should()
                .BeOfType<BuildingSnapshot>();
            var snapshot = _building.TakeSnapshot() as BuildingSnapshot;
            var expected =
                BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                    .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                        new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            {_.HuisNr16KoppelingId, _.HuisNr16Id}
                        })
                    .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>
                        {_.HuisNr16KoppelingId})
                    .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(
                        new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>,
                            List<AddressSubaddressWasImportedFromCrab>>
                        {
                            {
                                new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId,
                                    _.HuisNr16Id),
                                new List<AddressSubaddressWasImportedFromCrab>
                                {
                                    _importSubaddressFromCrab.ToLegacyEvent(),
                                    _importSubaddress2FromCrab.ToLegacyEvent()
                                }
                            }
                        })
                    .WithSubaddressStatusEventsBySubaddressId(
                        new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>
                        {
                            {
                                _.SubaddressNr16Bus1Id,
                                new List<AddressSubaddressStatusWasImportedFromCrab>
                                    {_importSubaddressStatusFromCrab.ToLegacyEvent()}
                            },
                            {
                                _.SubaddressNr16Bus2Id,
                                new List<AddressSubaddressStatusWasImportedFromCrab>
                                    {_importSubaddress2StatusFromCrab.ToLegacyEvent()}
                            }
                        })
                    .WithLastModificationFromCrab(Modification.Update)
                    .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                        .WithBuildingUnits(new List<BuildingUnitSnapshot>
                        {
                            BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded),

                            BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnit2WasAdded)
                                .WithStatus(BuildingUnitStatus.Realized)
                                .WithSubaddressStatusChronicle(new List<AddressSubaddressStatusWasImportedFromCrab>{_importSubaddressStatusFromCrab.ToLegacyEvent()}),

                            BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded)
                                .WithStatus(BuildingUnitStatus.Realized),

                            BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnit3WasAdded)
                                .WithStatus(BuildingUnitStatus.NotRealized)
                                .WithAddressIds(new List<AddressId>())
                                .WithSubaddressStatusChronicle(new List<AddressSubaddressStatusWasImportedFromCrab>{_importSubaddress2StatusFromCrab.ToLegacyEvent()})
                                .WithPreviousAddressId(_.Address16Bus2Id)
                        }));

            snapshot.Should().BeEquivalentTo(expected, config => config.AllowingInfiniteRecursion().IgnoringCyclicReferences());

            return _building.GetChanges().Count();
        }
    }
}

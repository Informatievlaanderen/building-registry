namespace BuildingRegistry.Tests.Cases
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building;
    using Building.Commands.Crab;
    using Building.Events;
    using FluentAssertions;
    using NetTopologySuite.IO;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.DataStructures;
    using Building.Events.Crab;
    using Namotion.Reflection;
    using ValueObjects;
    using WhenImportingCrabHouseNumberPosition;
    using WhenImportingCrabHouseNumberStatus;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabSubaddressPosition;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCaseBuffer : SnapshotBasedTest
    {
        private TestCaseData _;
        private Building _building;
        private ImportSubaddressFromCrab _importSubaddressFromCrab;
        private ImportSubaddressFromCrab _importSubaddressFromCrab2;
        private ImportSubaddressStatusFromCrab? _importSubaddressStatusFromCrab;
        private ImportSubaddressPositionFromCrab? _importSubaddressPositionFromCrab;
        private ImportSubaddressStatusFromCrab? _importSubaddress2StatusFromCrab;
        private ImportSubaddressPositionFromCrab? _importSubaddress2PositionFromCrab;
        private ImportHouseNumberStatusFromCrab? _houseNumberStatusFromCrab;
        private ImportHouseNumberPositionFromCrab? _importHouseNumberPositionFromCrab;

        public TestCaseBuffer(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            _ = new TestCaseData(Fixture);

            _building = Building.Register(_.Gebouw1Id, new BuildingFactory(IntervalStrategy.Default));
        }

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
                HuisNr16Id = customizedFixture.Create<CrabHouseNumberId>();
                SubaddressNr16Bus1Id = new CrabSubaddressId(161);
                SubaddressNr16Bus2Id = new CrabSubaddressId(162);
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
            public WkbGeometry BuildingGeometry => new WkbGeometry(new WKTReader().Read(BuildingWkt).AsBinary());
            public WkbGeometry CenterBuilding => new WkbGeometry(new WKTReader().Read(BuildingWkt).Centroid.AsBinary());
            public WkbGeometry HouseNrGeometry => new WkbGeometry(new WKTReader().Read(HousePositionWkt).AsBinary());
            public WkbGeometry NewHouseNrGeometry => new WkbGeometry(new WKTReader().Read(NewHousePositionWkt).AsBinary());
            public WkbGeometry Subaddr1Geometry => new WkbGeometry(new WKTReader().Read(Sub1PositionWkt).AsBinary());
            public WkbGeometry Subaddr2Geometry => new WkbGeometry(new WKTReader().Read(Sub2PositionWkt).AsBinary());
        }

        public int T0()
        {
            _importSubaddressStatusFromCrab = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.Proposed);

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
                .Skip(1)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _importSubaddressStatusFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        public int T1()
        {
            var skip = T0();

            _importSubaddressPositionFromCrab = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithPosition(_.Subaddr1Geometry);

            _building.ImportSubaddressPositionFromCrab(_importSubaddressPositionFromCrab.TerrainObjectId,
                _importSubaddressPositionFromCrab.TerrainObjectHouseNumberId,
                _importSubaddressPositionFromCrab.AddressPositionId,
                _importSubaddressPositionFromCrab.SubaddressId,
                _importSubaddressPositionFromCrab.AddressPosition,
                _importSubaddressPositionFromCrab.AddressPositionOrigin,
                _importSubaddressPositionFromCrab.AddressNature,
                _importSubaddressPositionFromCrab.Lifetime,
                _importSubaddressPositionFromCrab.Timestamp,
                _importSubaddressPositionFromCrab.Operator,
                _importSubaddressPositionFromCrab.Modification,
                _importSubaddressPositionFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _importSubaddressPositionFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        public int T2()
        {
            var skip = T1();

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

        public int T3()
        {
            var skip = T2();

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

        public int T4()
        {
            var skip = T3();

            _importSubaddress2PositionFromCrab = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithPosition(_.Subaddr2Geometry);

            _building.ImportSubaddressPositionFromCrab(_importSubaddress2PositionFromCrab.TerrainObjectId,
                _importSubaddress2PositionFromCrab.TerrainObjectHouseNumberId,
                _importSubaddress2PositionFromCrab.AddressPositionId,
                _importSubaddress2PositionFromCrab.SubaddressId,
                _importSubaddress2PositionFromCrab.AddressPosition,
                _importSubaddress2PositionFromCrab.AddressPositionOrigin,
                _importSubaddress2PositionFromCrab.AddressNature,
                _importSubaddress2PositionFromCrab.Lifetime,
                _importSubaddress2PositionFromCrab.Timestamp,
                _importSubaddress2PositionFromCrab.Operator,
                _importSubaddress2PositionFromCrab.Modification,
                _importSubaddress2PositionFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _importSubaddress2PositionFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        public int T5()
        {
            var skip = T4();

            _importSubaddressFromCrab2 = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _building.ImportSubaddressFromCrab(_importSubaddressFromCrab2.TerrainObjectId,
                _importSubaddressFromCrab2.TerrainObjectHouseNumberId,
                _importSubaddressFromCrab2.SubaddressId,
                _importSubaddressFromCrab2.HouseNumberId,
                _importSubaddressFromCrab2.BoxNumber,
                _importSubaddressFromCrab2.BoxNumberType,
                _importSubaddressFromCrab2.Lifetime,
                _importSubaddressFromCrab2.Timestamp,
                _importSubaddressFromCrab2.Operator,
                _importSubaddressFromCrab2.Modification,
                _importSubaddressFromCrab2.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(_importSubaddressFromCrab2.ToLegacyEvent());

            return _building.GetChanges().Count();
        }

        public int T6()
        {
            var skip = T5();

            _houseNumberStatusFromCrab = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithStatus(CrabAddressStatus.Proposed);

            _building.ImportHouseNumberStatusFromCrab(_houseNumberStatusFromCrab.TerrainObjectId,
                _houseNumberStatusFromCrab.TerrainObjectHouseNumberId,
                _houseNumberStatusFromCrab.HouseNumberStatusId,
                _houseNumberStatusFromCrab.HouseNumberId,
                _houseNumberStatusFromCrab.AddressStatus,
                _houseNumberStatusFromCrab.Lifetime,
                _houseNumberStatusFromCrab.Timestamp,
                _houseNumberStatusFromCrab.Operator,
                _houseNumberStatusFromCrab.Modification,
                _houseNumberStatusFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _houseNumberStatusFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        public int T7_BasedOnT5()
        {
            var skip = T5();

            var importHouseNumberPositionFromCrab = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(_.HuisNr16KoppelingId)
                .WithPosition(_.HouseNrGeometry);

            _building.ImportHouseNumberPositionFromCrab(importHouseNumberPositionFromCrab.TerrainObjectId,
                importHouseNumberPositionFromCrab.TerrainObjectHouseNumberId,
                importHouseNumberPositionFromCrab.AddressPositionId,
                importHouseNumberPositionFromCrab.HouseNumberId,
                importHouseNumberPositionFromCrab.AddressPosition,
                importHouseNumberPositionFromCrab.AddressPositionOrigin,
                importHouseNumberPositionFromCrab.AddressNature,
                importHouseNumberPositionFromCrab.Lifetime,
                importHouseNumberPositionFromCrab.Timestamp,
                importHouseNumberPositionFromCrab.Operator,
                importHouseNumberPositionFromCrab.Modification,
                importHouseNumberPositionFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    importHouseNumberPositionFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        public int T7()
        {
            var skip = T6();

            _importHouseNumberPositionFromCrab = Fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithTerrainObjectHouseNumber(_.HuisNr16KoppelingId)
                .WithPosition(_.HouseNrGeometry);

            _building.ImportHouseNumberPositionFromCrab(_importHouseNumberPositionFromCrab.TerrainObjectId,
                _importHouseNumberPositionFromCrab.TerrainObjectHouseNumberId,
                _importHouseNumberPositionFromCrab.AddressPositionId,
                _importHouseNumberPositionFromCrab.HouseNumberId,
                _importHouseNumberPositionFromCrab.AddressPosition,
                _importHouseNumberPositionFromCrab.AddressPositionOrigin,
                _importHouseNumberPositionFromCrab.AddressNature,
                _importHouseNumberPositionFromCrab.Lifetime,
                _importHouseNumberPositionFromCrab.Timestamp,
                _importHouseNumberPositionFromCrab.Operator,
                _importHouseNumberPositionFromCrab.Modification,
                _importHouseNumberPositionFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    _importHouseNumberPositionFromCrab.ToLegacyEvent()
                });

            return _building.GetChanges().Count();
        }

        public void T8_T7BasedOnT5()
        {
            var skip = T7_BasedOnT5();

            var importHouseNumberPositionFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _building.ImportTerrainObjectHouseNumberFromCrab(
                importHouseNumberPositionFromCrab.TerrainObjectHouseNumberId,
                importHouseNumberPositionFromCrab.TerrainObjectId,
                importHouseNumberPositionFromCrab.HouseNumberId,
                importHouseNumberPositionFromCrab.Lifetime,
                importHouseNumberPositionFromCrab.Timestamp,
                importHouseNumberPositionFromCrab.Operator,
                importHouseNumberPositionFromCrab.Modification,
                importHouseNumberPositionFromCrab.Organisation);

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importHouseNumberPositionFromCrab.Timestamp)),

                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importHouseNumberPositionFromCrab.Timestamp)),
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid2Id),

                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importHouseNumberPositionFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),

                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importHouseNumberPositionFromCrab.Timestamp)),
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid4Id),

                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),

                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),

                    importHouseNumberPositionFromCrab.ToLegacyEvent()
                }, config => config.WithStrictOrdering());
        }

        public void T8()
        {
            var skip = T7();

            var importHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            _building.ImportTerrainObjectHouseNumberFromCrab(
                importHouseNumberFromCrab.TerrainObjectHouseNumberId,
                importHouseNumberFromCrab.TerrainObjectId,
                importHouseNumberFromCrab.HouseNumberId,
                importHouseNumberFromCrab.Lifetime,
                importHouseNumberFromCrab.Timestamp,
                importHouseNumberFromCrab.Operator,
                importHouseNumberFromCrab.Modification,
                importHouseNumberFromCrab.Organisation);

            var buildingUnitWasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1Id, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importHouseNumberFromCrab.Timestamp));
            var buildingUnit2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importHouseNumberFromCrab.Timestamp));
            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importHouseNumberFromCrab.Timestamp));
            var buildingUnit3WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importHouseNumberFromCrab.Timestamp));

            _building.GetChanges()
                .Skip(skip)
                .Should()
                .BeEquivalentTo(new List<object>
                {
                    buildingUnitWasAdded,
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid1Id),

                    buildingUnit2WasAdded,
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid2Id),

                    commonBuildingUnitWasAdded,
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),

                    buildingUnit3WasAdded,
                    new BuildingUnitWasPlanned(_.Gebouw1Id, _.GebouwEenheid4Id),

                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),

                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),

                    importHouseNumberFromCrab.ToLegacyEvent()
                }, config => config.WithStrictOrdering());

            _building.TakeSnapshot()
                .Should()
                .BeOfType<BuildingSnapshot>()
                .And
                .Should()
                .BeEquivalentTo(
                    BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.HuisNr16KoppelingId, _.HuisNr16Id  }
                        })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{_.HuisNr16KoppelingId})
                        .WithHouseNumberPositionEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>
                        {
                            { _.Address16Id, new List<AddressHouseNumberPositionWasImportedFromCrab>{_importHouseNumberPositionFromCrab.ToLegacyEvent()}}
                        })
                        .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                        {
                            { _.Address16Id, new List<AddressHouseNumberStatusWasImportedFromCrab>{_houseNumberStatusFromCrab.ToLegacyEvent()} }
                        })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                            {
                                { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId, _.HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>
                                    { _importSubaddressFromCrab.ToLegacyEvent(), _importSubaddressFromCrab2.ToLegacyEvent()}
                                }
                            })
                        .WithSubaddressStatusEventsBySubaddressId(new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>
                        {
                            { _.SubaddressNr16Bus1Id, new List<AddressSubaddressStatusWasImportedFromCrab>{_importSubaddressStatusFromCrab.ToLegacyEvent()} },
                            { _.SubaddressNr16Bus2Id, new List<AddressSubaddressStatusWasImportedFromCrab>{_importSubaddress2StatusFromCrab.ToLegacyEvent()} }
                        })
                        .WithSubaddressPositionEventsBySubaddressId(new Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>>
                        {
                            { _.SubaddressNr16Bus1Id, new List<AddressSubaddressPositionWasImportedFromCrab>{_importSubaddressPositionFromCrab.ToLegacyEvent()} },
                            { _.SubaddressNr16Bus2Id, new List<AddressSubaddressPositionWasImportedFromCrab>{ _importSubaddress2PositionFromCrab.ToLegacyEvent()} }
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address16Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnit2WasAdded)
                                    .WithStatus(BuildingUnitStatus.Planned),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithAddressIds(new List<AddressId>{_.Address16Id}),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnit3WasAdded)
                                    .WithStatus(BuildingUnitStatus.Planned)
                            }))
                    , config => config.AllowingInfiniteRecursion().ExcludingMissingMembers());
        }

        [Fact]
        public void TestT8()
        {
            T8();
        }

        [Fact]
        public void TestWithoutHouseNumberStatus()
        {
            T8_T7BasedOnT5();
        }
    }
}

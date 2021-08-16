namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnit : SnapshotBasedTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
            Fixture.Customize(new WithValidPoint());
        }

        [Fact]
        public void WithValidGeometryWhenUnitHasGeometry()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var position = new WkbGeometry(wkbBytes);

            var command = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(position);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var addressHouseNumberPositionWasImportedFromCrab = Fixture
                .Create<AddressHouseNumberPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>(),
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(position))),
                    new Fact(buildingId,
                        new BuildingUnitPositionWasAppointedByAdministrator(buildingId,
                            Fixture.Create<BuildingUnitId>(),
                            ExtendedWkbGeometry.CreateEWkb(Fixture.Create<WkbGeometry>()))),
                    new Fact(buildingId, new BuildingUnitBecameComplete(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithGeometry(new BuildingGeometry(GeometryHelper.CreateEwkbFrom(position),
                                BuildingGeometryMethod.Outlined))
                            .WithGeometryChronicle(command)
                            .WithHouseNumberPositionEventsByHouseNumberId(
                                new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>
                                {
                                    {
                                        AddressId.CreateFor(new CrabHouseNumberId(
                                            addressHouseNumberPositionWasImportedFromCrab.HouseNumberId)),
                                        new List<AddressHouseNumberPositionWasImportedFromCrab>
                                        {
                                            addressHouseNumberPositionWasImportedFromCrab,
                                        }
                                    }
                                })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithPosition(new BuildingUnitPosition(
                                            ExtendedWkbGeometry.CreateEWkb(Fixture.Create<WkbGeometry>()),
                                            BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                                        .WithHouseNumberPositions(
                                            new List<AddressHouseNumberPositionWasImportedFromCrab>
                                            {
                                                addressHouseNumberPositionWasImportedFromCrab,
                                            })
                                        .WithStatus(BuildingUnitStatus.Realized)
                                        .BecameComplete(true)
                                }))
                            .WithLastModificationFromCrab(Modification.Update)
                            .BecameComplete(false)
                            .Build(7, EventSerializerSettings))
                }));
        }

        // Test if adding different geometry where unit is now outside = invalid = remove position from unit
        // Also test to change geometry with common unit => common unit also changes position
        [Fact]
        public void WithNewGeometryWhereUnitsFallOutOf()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var wkbBytes = GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary();
            var command = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(new WkbGeometry(wkbBytes));

            var buildingId = Fixture.Create<BuildingId>();
            var commonUnitKey = BuildingUnitKey.Create(Fixture.Create<CrabTerrainObjectId>());
            var commonUnitId = BuildingUnitId.Create(commonUnitKey, 1);
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitKey(commonUnitKey)
                .WithBuildingUnitId(commonUnitId);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    commonBuildingUnitWasAdded,
                    Fixture.Create<BuildingUnitPositionWasDerivedFromObject>()
                        .WithBuildingUnitId(commonUnitId)
                        .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.Centroid.AsBinary())))
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId,
                        new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes)))),
                    new Fact(buildingId,
                        new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(),
                            GeometryHelper.CreateEwkbFrom(
                                new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.Centroid.AsBinary())))),
                    new Fact(buildingId,
                        new BuildingUnitPositionWasDerivedFromObject(buildingId, commonUnitId,
                            GeometryHelper.CreateEwkbFrom(
                                new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.Centroid.AsBinary())))),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithGeometry(new BuildingGeometry(GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes)),
                                BuildingGeometryMethod.Outlined))
                            .WithGeometryChronicle(command)
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithPosition(new BuildingUnitPosition(
                                            GeometryHelper.CreateEwkbFrom(new WkbGeometry(GeometryHelper
                                                .ValidPolygonWithNoValidPoints.Centroid.AsBinary())),
                                            BuildingUnitPositionGeometryMethod.DerivedFromObject)),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded)
                                        .WithPosition(new BuildingUnitPosition(
                                            GeometryHelper.CreateEwkbFrom(new WkbGeometry(GeometryHelper
                                                .ValidPolygonWithNoValidPoints.Centroid.AsBinary())),
                                            BuildingUnitPositionGeometryMethod.DerivedFromObject))
                                }))
                            .WithLastModificationFromCrab(Modification.Insert)
                            .Build(9, EventSerializerSettings))
                }));
        }

        // Test if adding unit geometry inside new polygon and then add new polygon => expect first was derived, then with new was appointed
        [Fact]
        public void WithNewGeometryWhereUnitsFallBackIn()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var polygonFixture = new Fixture().Customize(new WithValidPolygonOutsideValidPointBoundary());
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var position = new WkbGeometry(wkbBytes);
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var addressHouseNumberPositionWasImportedFromCrab = Fixture
                .Create<AddressHouseNumberPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            var command = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(position);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingWasMeasuredByGrb>().WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasDerivedFromObject>(),
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId,
                        new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes)))),
                    new Fact(buildingId,
                        new BuildingUnitPositionWasAppointedByAdministrator(buildingId,
                            Fixture.Create<BuildingUnitId>(),
                            ExtendedWkbGeometry.CreateEWkb(Fixture.Create<WkbGeometry>()))),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithGeometry(new BuildingGeometry(GeometryHelper.CreateEwkbFrom(position),
                                BuildingGeometryMethod.Outlined))
                            .WithGeometryChronicle(command)
                            .WithHouseNumberPositionEventsByHouseNumberId(
                                new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>
                                {
                                    {
                                        AddressId.CreateFor(new CrabHouseNumberId(
                                            addressHouseNumberPositionWasImportedFromCrab.HouseNumberId)),
                                        new List<AddressHouseNumberPositionWasImportedFromCrab>
                                        {
                                            addressHouseNumberPositionWasImportedFromCrab
                                        }
                                    }
                                })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithPosition(new BuildingUnitPosition(
                                            ExtendedWkbGeometry.CreateEWkb(Fixture.Create<WkbGeometry>()),
                                            BuildingUnitPositionGeometryMethod.AppointedByAdministrator))
                                        .WithHouseNumberPositions(
                                            new List<AddressHouseNumberPositionWasImportedFromCrab>
                                            {
                                                addressHouseNumberPositionWasImportedFromCrab
                                            })
                                }))
                            .WithLastModificationFromCrab(Modification.Update)
                            .Build(7, EventSerializerSettings))
                }));
        }

        // Test if adding unit geometry inside new polygon and then add new polygon => expect first was derived, then with new was appointed
        [Fact]
        public void WithNewGeometryWhereUnitsFallBackInAndUnitIsRetired()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygonOutsideValidPointBoundary());
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var command = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(new WkbGeometry(wkbBytes));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasDerivedFromObject>(),
                    Fixture.Create<BuildingUnitWasRetired>(),
                    Fixture.Create<BuildingUnitBecameComplete>(),
                    Fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes))),
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, Fixture.Create<BuildingUnitId>(),
                        GeometryHelper.CreateEwkbFrom(Fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithModificationDelete()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var addressHouseNumberPositionWasImportedFromCrab =
                Fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>();


            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var position = new WkbGeometry(wkbBytes);
            var command = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(position)
                .WithCrabModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingWasMeasuredByGrb>().WithGeometry(position),
                    Fixture.Create<BuildingUnitPositionWasDerivedFromObject>(),
                    addressHouseNumberPositionWasImportedFromCrab,
                    Fixture.Create<BuildingUnitBecameComplete>())
                .When(command)
                .Then(new Fact[]
                {
                    new Fact(buildingId, new BuildingGeometryWasRemoved(buildingId)),
                    new Fact(buildingId,
                        new BuildingUnitBecameIncomplete(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, command.ToLegacyEvent()),

                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithGeometryChronicle(command)
                            .WithHouseNumberPositionEventsByHouseNumberId(
                                new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>
                                {
                                    {
                                        AddressId.CreateFor(new CrabHouseNumberId(
                                            addressHouseNumberPositionWasImportedFromCrab.HouseNumberId)),
                                        new List<AddressHouseNumberPositionWasImportedFromCrab>
                                        {
                                            addressHouseNumberPositionWasImportedFromCrab
                                        }
                                    }
                                })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .BecameComplete(false)
                                        .WithHouseNumberPositions(
                                            new List<AddressHouseNumberPositionWasImportedFromCrab>
                                            {
                                                addressHouseNumberPositionWasImportedFromCrab
                                            })
                                }))
                            .WithIsRemoved(false)
                            .WithLastModificationFromCrab(Modification.Update)
                            .Build(8, EventSerializerSettings))
                }));
        }
    }
}

namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using Building.Events.Crab;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnit : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
            _fixture.Customize(new WithValidPoint());
        }

        [Fact]
        public void WithValidGeometryWhenUnitHasGeometry()
        {
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();

            var command = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(new WkbGeometry(wkbBytes));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasRealized>(),
                    _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>()
                        .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand))
                .When(command)
                .Then(buildingId,
                    new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes))),
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), ExtendedWkbGeometry.CreateEWkb(_fixture.Create<WkbGeometry>())),
                    new BuildingUnitBecameComplete(buildingId, _fixture.Create<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        // Test if adding different geometry where unit is now outside = invalid = remove position from unit
        // Also test to change geometry with common unit => common unit also changes position
        [Fact]
        public void WithNewGeometryWhereUnitsFallOutOf()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var wkbBytes = GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary();
            var command = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(new WkbGeometry(wkbBytes));

            var buildingId = _fixture.Create<BuildingId>();
            var commonUnitKey = BuildingUnitKey.Create(_fixture.Create<CrabTerrainObjectId>());
            var commonUnitId = BuildingUnitId.Create(commonUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitKey(commonUnitKey)
                        .WithBuildingUnitId(commonUnitId),
                    _fixture.Create<BuildingUnitPositionWasDerivedFromObject>()
                        .WithBuildingUnitId(commonUnitId)
                        .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.Centroid.AsBinary())))
                .When(command)
                .Then(buildingId,
                    new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes))),
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.Centroid.AsBinary()))),
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, commonUnitId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.Centroid.AsBinary()))),
                    command.ToLegacyEvent()));
        }

        // Test if adding unit geometry inside new polygon and then add new polygon => expect first was derived, then with new was appointed
        [Fact]
        public void WithNewGeometryWhereUnitsFallBackIn()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygonOutsideValidPointBoundary());
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var command = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(new WkbGeometry(wkbBytes));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasDerivedFromObject>(),
                    _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes))),
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(_fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        // Test if adding unit geometry inside new polygon and then add new polygon => expect first was derived, then with new was appointed
        [Fact]
        public void WithNewGeometryWhereUnitsFallBackInAndUnitIsRetired()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygonOutsideValidPointBoundary());
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var command = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithGeometry(new WkbGeometry(wkbBytes));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasDerivedFromObject>(),
                    _fixture.Create<BuildingUnitWasRetired>(),
                    _fixture.Create<BuildingUnitBecameComplete>(),
                    _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasOutlined(buildingId, GeometryHelper.CreateEwkbFrom(new WkbGeometry(wkbBytes))),
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(_fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithModificationDelete()
        {
            var wkbBytes = GeometryHelper.ValidPolygon.AsBinary();
            var command = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithGeometry(new WkbGeometry(wkbBytes))
                .WithCrabModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())),
                    _fixture.Create<BuildingUnitPositionWasDerivedFromObject>(),
                    _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>(),
                    _fixture.Create<BuildingUnitBecameComplete>())
                .When(command)
                .Then(buildingId,
                    new BuildingGeometryWasRemoved(buildingId),
                    new BuildingUnitBecameIncomplete(buildingId, _fixture.Create<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }
    }
}

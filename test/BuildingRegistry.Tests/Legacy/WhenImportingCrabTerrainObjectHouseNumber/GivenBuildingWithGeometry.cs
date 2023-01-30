namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using NetTopologySuite.IO;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithGeometry : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingWithGeometry(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithValidPolygon());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void ThenBuildingUnitIsAddedWithDerivedGeometry()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var center = new WKBReader().Read(_fixture.Create<WkbGeometry>()).Centroid;

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(_fixture.Create<WkbGeometry>()))
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, _fixture.Create<BuildingUnitId>(), _fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary()))),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeThenRetireNewlyAddedBuildingUnitAndDerivePosition()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var center = new WKBReader().Read(_fixture.Create<WkbGeometry>()).Centroid;

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(_fixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAddedToRetiredBuilding(buildingId, _fixture.Create<BuildingUnitId>(), _fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary()))),
                    command.ToLegacyEvent()));
        }
    }
}

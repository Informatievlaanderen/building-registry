namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitRetirement
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Tests.Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingStatus = Building.BuildingStatus;

    public class GivenBuildingUnitOutsideOfBuilding : BuildingRegistryTest
    {
        public GivenBuildingUnitOutsideOfBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void ThenCenterBuildingUnit()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var pointNotInPolygon =
                new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary());

            var migrateScenario = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: pointNotInPolygon)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    migrateScenario)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }
    }
}

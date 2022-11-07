namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitRetirement
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenCommonBuildingUnit : BuildingRegistryTest
    {
        public GivenCommonBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithRetiredCommonBuildingUnit_ThenCommonBuildingBecomesRealized()
        {
            var command = new CorrectBuildingUnitRetirement(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var commonBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(2);

            var migrateScenario = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    commonBuildingUnitPersistentLocalId,
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(3),
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    migrateScenario)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            null)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId,
                            null))));
        }

        [Fact]
        public void WithOutsideBuildingPosition_ThenCenter()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var migrateScenario = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.ToBinary()))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    commonBuildingUnitPersistentLocalId,
                    BuildingRegistry.Legacy.BuildingUnitFunction.Common)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .Build();

            var correctCommonBuildingUnitPosition = new BuildingUnitPositionWasCorrected(
                command.BuildingPersistentLocalId,
                commonBuildingUnitPersistentLocalId,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.ToBinary()));
            ((ISetProvenance)correctCommonBuildingUnitPosition).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    migrateScenario,
                    correctCommonBuildingUnitPosition)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId,
                            buildingGeometry.Center))));
        }

        [Fact]
        public void WithoutCommonBuildingUnitWithRealizedBuilding_ThenCommonBuildingUnitIsAdded()
        {
            var command = new CorrectBuildingUnitRetirement(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(BuildingRegistry.Legacy.BuildingUnitStatus.Planned, new BuildingUnitPersistentLocalId(3))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            null)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Realized,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }
    }
}

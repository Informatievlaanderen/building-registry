#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitRemoval
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
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenCommonBuildingUnit : BuildingRegistryTest
    {
        public GivenCommonBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customizations.Add(new WithUniqueInteger(2));
        }

        [Fact]
        public void WithRetiredCommonBuildingUnit_ThenCommonBuildingBecomesRealized()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingStatus = BuildingStatus.Realized;
            var buildingUnitStatus = BuildingUnitStatus.Realized;
            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    buildingUnitStatus,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    isRemoved: true)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitRemovalWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitStatus.Status),
                            BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithOutsideOfBuildingPosition_ThenCenter()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingStatus = BuildingStatus.Realized;
            var buildingUnitStatus = BuildingUnitStatus.Realized;
            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    buildingUnitStatus,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    isRemoved: true)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            var buildingUnitPositionWasCorrected = new BuildingUnitPositionWasCorrected(
                command.BuildingPersistentLocalId,
                commonBuildingUnitPersistentLocalId,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.ToBinary()));
            ((ISetProvenance)buildingUnitPositionWasCorrected).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated,
                    buildingUnitPositionWasCorrected)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitRemovalWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitStatus.Status),
                            BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithoutCommonBuildingUnitAndRealizedBuilding_ThenCommonBuildingUnitIsAdded()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(BuildingUnitStatus.Planned)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitRemovalWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingRegistry.Building.BuildingUnitStatus.Planned,
                            BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingRegistry.Building.BuildingUnitStatus.Realized,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }
    }
}

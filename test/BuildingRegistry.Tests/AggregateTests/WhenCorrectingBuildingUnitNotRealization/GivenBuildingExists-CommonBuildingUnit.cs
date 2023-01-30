namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitNotRealization
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Extensions;
    using Xunit;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public partial class GivenBuildingExists
    {
        [Theory]
        [InlineData("Planned", "Planned")]
        [InlineData("UnderConstruction", "Planned")]
        [InlineData("Realized", "Realized")]
        public void WithNoCommonBuildingUnit_ThenCommonBuildingUnitIsAdded(string buildingStatus, string expectedStatus)
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(3))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
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
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingRegistry.Building.BuildingUnitStatus.Parse(expectedStatus),
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            hasDeviation: false))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        public void WithNotRealizedCommonBuildingUnitAndBuildingWithStatus_ThenCommonBuildingUnitIsPlanned(string buildingStatus)
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>()
            );

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(BuildingUnitStatus.Planned, new BuildingUnitPersistentLocalId(2))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(3),
                    BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(3)))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        public void WithNotRealizedCommonBuildingUnitWithOutdatedPositionAndBuildingWithStatus_ThenCommonBuildingUnitIsPlannedAndPositionCorrected(string buildingStatus)
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(3))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .Build();

            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAddedV2(
                command.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(1),
                BuildingRegistry.Building.BuildingUnitStatus.NotRealized,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                false);
            ((ISetProvenance)commonBuildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated,
                    commonBuildingUnitWasAdded)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1)))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        public void WithoutCommonBuildingUnitWithBuildingStatus_ThenCommonBuildingUnitIsAdded(string buildingStatus)
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(BuildingUnitStatus.Planned, new BuildingUnitPersistentLocalId(3))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
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
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingRegistry.Building.BuildingUnitStatus.Planned,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithNotRealizedCommonBuildingUnitAndRealizedBuilding_ThenCommonBuildingUnitIsRealized()
        {
            var command = new CorrectBuildingUnitNotRealization(
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
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(3),
                    BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(3))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(3)))));
        }

        [Fact]
        public void WithNotRealizedCommonBuildingUnitWithOutdatedPositionAndRealizedBuilding_ThenCommonBuildingUnitIsRealizedAndPositionCorrected()
        {
            var command = new CorrectBuildingUnitNotRealization(
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
                .WithBuildingUnit(BuildingUnitStatus.Planned)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .Build();

            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAddedV2(
                command.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(1),
                BuildingRegistry.Building.BuildingUnitStatus.NotRealized,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                false);
            ((ISetProvenance)commonBuildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated,
                    commonBuildingUnitWasAdded)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1)))));
        }

        [Fact]
        public void WithoutCommonBuildingUnitWithRealizedBuilding_ThenCommonBuildingUnitWasAdded()
        {
            var command = new CorrectBuildingUnitNotRealization(
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
                .WithBuildingUnit(BuildingUnitStatus.Planned, new BuildingUnitPersistentLocalId(3))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
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
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingRegistry.Building.BuildingUnitStatus.Realized,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithRetiredCommonBuildingUnitAndRealizedBuilding_ThenCommonBuildingUnitIsRealized()
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(buildingGeometry.Center.ToString()))
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    new BuildingUnitPersistentLocalId(3),
                    BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(3)))));
        }

        [Fact]
        public void WhenRetiredCommonBuildingUnitHasOutdatedPosition_ThenCommonBuildingUnitIsRealizedAndPositionCorrected()
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var initialBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(initialBuildingGeometry)
                .WithBuildingUnit(BuildingUnitStatus.Planned, new BuildingUnitPersistentLocalId(3))
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(initialBuildingGeometry.Center.ToString()))
                .Build();

            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAddedV2(
                command.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(1),
                BuildingRegistry.Building.BuildingUnitStatus.Retired,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                false);
            ((ISetProvenance)commonBuildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated,
                    commonBuildingUnitWasAdded)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            initialBuildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1)))));
        }
    }
}

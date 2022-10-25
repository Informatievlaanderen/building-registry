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
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

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

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                    .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                    .WithBuildingStatus(buildingStatus)
                    .WithBuildingUnit(BuildingUnitStatus.Planned)
                    .WithBuildingUnit(BuildingUnitStatus.NotRealized, command.BuildingUnitPersistentLocalId)
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
                        new BuildingGeometry(
                            new ExtendedWkbGeometry(buildingWasMigrated.ExtendedWkbGeometry),
                            BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod)).Center,
                        hasDeviation: false))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        public void WithNotRealizedCommonBuildingUnitAndBuildingWithStatus_ThenCommonBuildingUnitIsPlanned(string buildingStatus)
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(buildingStatus)
                .WithBuildingUnit(BuildingUnitStatus.Planned)
                .WithBuildingUnit(BuildingUnitStatus.NotRealized, command.BuildingUnitPersistentLocalId)
                .WithBuildingUnit(BuildingUnitStatus.NotRealized, new BuildingUnitPersistentLocalId(1), BuildingUnitFunction.Common)
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
                        new BuildingUnitPersistentLocalId(1)))));
        }

        [Fact]
        public void WithNotRealizedCommonBuildingUnitAndRealizedBuilding_ThenCommonBuildingUnitIsRealized()
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(BuildingUnitStatus.Planned)
                .WithBuildingUnit(BuildingUnitStatus.NotRealized, command.BuildingUnitPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(1),
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
                        new BuildingUnitPersistentLocalId(1))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasRealizedV2(
                        command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(1)))));
        }

        [Fact]
        public void WithRetiredCommonBuildingUnitAndRealizedBuilding_ThenCommonBuildingUnitIsRealized()
        {
            var command = new CorrectBuildingUnitNotRealization(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>()
            );

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(BuildingUnitStatus.Planned)
                .WithBuildingUnit(BuildingUnitStatus.NotRealized, command.BuildingUnitPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    new BuildingUnitPersistentLocalId(1),
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
                            new BuildingUnitPersistentLocalId(1)))));
        }
    }
}

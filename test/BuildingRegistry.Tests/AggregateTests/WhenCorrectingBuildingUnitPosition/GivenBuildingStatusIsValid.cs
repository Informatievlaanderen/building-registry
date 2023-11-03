namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitPosition
{
    using System.Collections.Generic;
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
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class GivenBuildingStatusIsValid : BuildingRegistryTest
    {
        public GivenBuildingStatusIsValid(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        [InlineData("Realized")]
        public void ThenBuildingUnitWasCorrected(string buildingStatus)
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Parse(buildingStatus))
                .Build();

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasMigrated.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitPositionWasCorrected(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        buildingGeometry.Center))));
        }
    }
}

namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class GivenBuildingStatusIsNotValid : BuildingRegistryTest
    {
        public GivenBuildingStatusIsNotValid(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void ThenThrowBuildingUnitCannotBePlannedException(string buildingStatus)
        {
            var command = Fixture.Create<PlanBuildingUnit>().WithoutPosition();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(buildingStatus),
                Fixture.Create<BuildingGeometry>(),
                false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }
    }
}

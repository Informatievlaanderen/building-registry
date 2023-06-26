namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenNewBuildingAlreadyExists : BuildingRegistryTest
    {
        public GivenNewBuildingAlreadyExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenAggregateSourceExceptionIsThrown()
        {
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            var command = new MergeBuildings(
                new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
                Fixture.Create<ExtendedWkbGeometry>(),
                new List<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.NewBuildingPersistentLocalId),
                    buildingWasPlanned)
                .When(command)
                .Throws(new AggregateSourceException(
                    $"Building with id {command.NewBuildingPersistentLocalId} already exists")));
        }
    }
}

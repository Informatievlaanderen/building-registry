namespace BuildingRegistry.Tests.AggregateTests.WhenRealizingUnplannedBuildingUnit
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithValidExtendedWkbPolygon());
        }

        [Fact]
        public void ThenUnplannedBuildingWasRealizedAndMeasured()
        {
            var command = Fixture.Create<RealizeUnplannedBuildingUnit>();

            var unplannedBuildingWasRealizedAndMeasured = Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();
            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(unplannedBuildingWasRealizedAndMeasured.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    unplannedBuildingWasRealizedAndMeasured)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasPlannedV2(command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            BuildingUnitFunction.Unknown,
                            false)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasAttachedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId,
                            command.AddressPersistentLocalId))));
        }
    }
}

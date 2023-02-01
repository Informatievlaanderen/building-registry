namespace BuildingRegistry.Tests.AggregateTests.WhenAttachingAddressToBuildingUnit
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Exceptions;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitRemoved : BuildingRegistryTest
    {
        public GivenBuildingUnitRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowBuildingUnitRemovedException()
        {
            var command = Fixture.Create<AttachAddressToBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    null,
                    null,
                    null,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }
    }
}

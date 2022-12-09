namespace BuildingRegistry.Tests.AggregateTests.WhenAttachingAddressToBuildingUnit
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building.Commands;
    using Building.Exceptions;
    using Building;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitStatusInvalid : BuildingRegistryTest
    {
        public GivenBuildingUnitStatusInvalid(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void ThenThrowBuildingUnitRemovedException(string status)
        {
            var command = Fixture.Create<AttachAddressToBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Parse(status).Value,
                    command.BuildingUnitPersistentLocalId,
                    null,
                    null,
                    null,
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidStatusException()));
        }
    }
}

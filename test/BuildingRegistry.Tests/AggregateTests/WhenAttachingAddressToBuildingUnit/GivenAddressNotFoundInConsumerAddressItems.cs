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

    public class GivenAddressNotFoundInConsumerAddressItems : BuildingRegistryTest
    {
        public GivenAddressNotFoundInConsumerAddressItems(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<AttachAddressToBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Throws(new AddressNotFoundException()));
        }
    }
}

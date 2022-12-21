namespace BuildingRegistry.Tests.AggregateTests.WhenDetachingAddressFromBuildingUnit
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Tests.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressNotFoundInConsumerAddressItems : BuildingRegistryTest
    {
        public GivenAddressNotFoundInConsumerAddressItems(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowsAddressNotFoundException()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    null,
                    null,
                    null,
                    new List<AddressPersistentLocalId>() { command.AddressPersistentLocalId },
                    isRemoved: false)
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

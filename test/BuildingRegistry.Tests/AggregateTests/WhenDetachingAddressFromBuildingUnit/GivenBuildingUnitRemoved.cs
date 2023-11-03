namespace BuildingRegistry.Tests.AggregateTests.WhenDetachingAddressFromBuildingUnit
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Exceptions;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingUnitRemoved : BuildingRegistryTest
    {
        public GivenBuildingUnitRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowBuildingUnitRemovedException()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
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

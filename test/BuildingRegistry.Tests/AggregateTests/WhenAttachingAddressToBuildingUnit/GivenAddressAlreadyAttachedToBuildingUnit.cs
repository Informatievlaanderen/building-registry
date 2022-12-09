namespace BuildingRegistry.Tests.AggregateTests.WhenAttachingAddressToBuildingUnit
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressAlreadyAttachedToBuildingUnit : BuildingRegistryTest
    {
        public GivenAddressAlreadyAttachedToBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenDoNothing()
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
                    new List<AddressPersistentLocalId>{command.AddressPersistentLocalId},
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }
    }
}

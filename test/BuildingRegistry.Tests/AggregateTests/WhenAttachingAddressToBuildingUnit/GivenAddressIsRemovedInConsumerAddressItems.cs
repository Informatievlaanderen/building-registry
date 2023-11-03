namespace BuildingRegistry.Tests.AggregateTests.WhenAttachingAddressToBuildingUnit
{
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Exceptions;
    using Consumer.Address;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenAddressIsRemovedInConsumerAddressItems : BuildingRegistryTest
    {
        public GivenAddressIsRemovedInConsumerAddressItems(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowsAddressIsRemovedException()
        {
            var command = Fixture.Create<AttachAddressToBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.AddressPersistentLocalId, AddressStatus.Current, isRemoved: true);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Throws(new AddressIsRemovedException()));
        }
    }
}

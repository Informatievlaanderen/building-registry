namespace BuildingRegistry.Tests.AggregateTests.WhenDetachingAddressFromBuildingUnit
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Tests.BackOffice;
    using BuildingRegistry.Tests.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemovedInConsumerAddressItems : BuildingRegistryTest
    {
        public GivenAddressIsRemovedInConsumerAddressItems(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenThrowsAddressIsRemovedException()
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
                    new List<AddressPersistentLocalId>(){command.AddressPersistentLocalId},
                    isRemoved: false)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.AddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: true);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Throws(new AddressIsRemovedException()));
        }
    }
}

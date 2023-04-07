namespace BuildingRegistry.Tests.AggregateTests.WhenReplacingAddressAttachmentFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BackOffice;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressReaddressed : BuildingRegistryTest
    {
        public GivenAddressReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenBuildingUnitAddressAttachmentWasReplacedBecauseAddressWasReaddressed()
        {
            var command = Fixture.Create<ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>(){ command.PreviousAddressPersistentLocalId },
                    isRemoved: false)
                .Build();
            
            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.PreviousAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);
            consumerAddress.AddAddress(command.NewAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            var buildingUnit = buildingWasMigrated.BuildingUnits.First();
            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                   new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                       new BuildingPersistentLocalId(buildingWasMigrated.BuildingPersistentLocalId),
                       new BuildingUnitPersistentLocalId(buildingUnit.BuildingUnitPersistentLocalId),
                       new AddressPersistentLocalId(command.PreviousAddressPersistentLocalId),
                       new AddressPersistentLocalId(command.NewAddressPersistentLocalId)
                       ))));
        }

        [Fact]
        public void WithAlreadyReplacedAttachment_ThenBuildingUnitAddressAttachmentWasReplacedBecauseAddressWasReaddressed()
        {
            var command = Fixture.Create<ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>(){ command.NewAddressPersistentLocalId },
                    isRemoved: false)
                .Build();
            
            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.PreviousAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);
            consumerAddress.AddAddress(command.NewAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);


            var buildingUnit = buildingWasMigrated.BuildingUnits.First();
            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }
    }
}

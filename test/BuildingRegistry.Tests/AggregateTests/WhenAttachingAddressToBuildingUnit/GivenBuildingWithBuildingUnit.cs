namespace BuildingRegistry.Tests.AggregateTests.WhenAttachingAddressToBuildingUnit
{
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building.Events;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;

    public class GivenBuildingWithBuildingUnit : BuildingRegistryTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenApplyBuildingUnitAddressWasAttachedV2()
        {
            var command = Fixture.Create<AttachAddressToBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    isRemoved: false)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.AddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitAddressWasAttachedV2(command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId, command.AddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedV2(
                new BuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>()),
                new BuildingUnitPersistentLocalId(Fixture.Create<BuildingUnitPersistentLocalId>()),
                new AddressPersistentLocalId(Fixture.Create<AddressPersistentLocalId>()));
            ((ISetProvenance)buildingUnitAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(new BuildingPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingPersistentLocalId))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                    isRemoved: false)
                .Build();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId), Consumer.Address.AddressStatus.Current, isRemoved: false);

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasAttachedV2
            });


            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().BeEquivalentTo(new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId) });
        }
    }
}

namespace BuildingRegistry.Tests.AggregateTests.WhenDetachingAddressFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Tests.BackOffice;
    using BuildingRegistry.Tests.Extensions;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnit : BuildingRegistryTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenApplyBuildingUnitAddressWasDetachedV2()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnit>();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.AddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    isRemoved: false)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitAddressWasDetachedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.AddressPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitAddressWasDetachedV2 = new BuildingUnitAddressWasDetachedV2(
                new BuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>()),
                new BuildingUnitPersistentLocalId(Fixture.Create<BuildingUnitPersistentLocalId>()),
                new AddressPersistentLocalId(Fixture.Create<AddressPersistentLocalId>()));
            ((ISetProvenance)buildingUnitAddressWasDetachedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(new BuildingPersistentLocalId(buildingUnitAddressWasDetachedV2.BuildingPersistentLocalId))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId),
                    attachedAddresses: new List<AddressPersistentLocalId>
                    {
                        new AddressPersistentLocalId(buildingUnitAddressWasDetachedV2.AddressPersistentLocalId),
                        new AddressPersistentLocalId(123)
                    },
                    isRemoved: false)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasDetachedV2
            });

            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().BeEquivalentTo(
                new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(123) });
        }
    }
}

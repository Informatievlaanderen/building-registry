namespace BuildingRegistry.Tests.AggregateTests.WhenDetachingAddressFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Consumer.Address;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingWithBuildingUnit : BuildingRegistryTest
    {
        public GivenBuildingWithBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenApplyBuildingUnitAddressWasDetachedV2()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnit>();

            var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
            consumerAddress.AddAddress(command.AddressPersistentLocalId, AddressStatus.Current, isRemoved: false);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
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

            var expectedPersistentLocalId = buildingUnitAddressWasDetachedV2.AddressPersistentLocalId + 1;
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(new BuildingPersistentLocalId(buildingUnitAddressWasDetachedV2.BuildingPersistentLocalId))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId),
                    attachedAddresses: new List<AddressPersistentLocalId>
                    {
                        new AddressPersistentLocalId(buildingUnitAddressWasDetachedV2.AddressPersistentLocalId),
                        new AddressPersistentLocalId(expectedPersistentLocalId)
                    },
                    isRemoved: false)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated,
                buildingUnitAddressWasDetachedV2
            });

            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().BeEquivalentTo(
                new List<AddressPersistentLocalId>{ new AddressPersistentLocalId(expectedPersistentLocalId) });
        }

        [Fact]
        public void WithUnusedCommonUnit_ThenApplyBuildingUnitAddressWasDetachedV2()
        {
            var command = Fixture.Create<DetachAddressFromBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId + 1),
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    function: BuildingUnitFunction.Common,
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId> { command.AddressPersistentLocalId },
                    function: BuildingUnitFunction.Common,
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
    }
}

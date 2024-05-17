namespace BuildingRegistry.Tests.AggregateTests.WhenReaddressingAddresses
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Events;
    using BuildingRegistry.Building.Commands;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }
        
        [Fact]
        public void WithSourceAddressAttachedAndDestinationAddressNotAttached_ThenAttachAndDetach()
        {
            var firstBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var secondBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(2);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(firstBuildingUnitPersistentLocalId, sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .WithReaddress(secondBuildingUnitPersistentLocalId, sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(firstBuildingUnitPersistentLocalId)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .WithAddress(2)
                    .Build())
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(secondBuildingUnitPersistentLocalId)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .Build())
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(3)
                    .WithAddress(3)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingBuildingUnitsAddressesWereReaddressed(
                        command.BuildingPersistentLocalId,
                        new[]
                        {
                            new BuildingUnitAddressesWereReaddressed(
                                firstBuildingUnitPersistentLocalId,
                                new[] { destinationAddressPersistentLocalId },
                                new[] { sourceAddressPersistentLocalId }
                            ),
                            new BuildingUnitAddressesWereReaddressed(
                                secondBuildingUnitPersistentLocalId,
                                new[] { destinationAddressPersistentLocalId },
                                new[] { sourceAddressPersistentLocalId }
                            )
                        },
                        command.Readdresses
                            .SelectMany(x => x.Value)
                            .Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void WithSourceAddressAttachedAndDestinationAddressAlreadyAttached_ThenOnlyDetach()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(buildingUnitPersistentLocalId, sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(2)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .WithAddress(destinationAddressPersistentLocalId)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingBuildingUnitsAddressesWereReaddressed(
                        command.BuildingPersistentLocalId,
                        new[]
                        {
                            new BuildingUnitAddressesWereReaddressed(
                                buildingUnitPersistentLocalId,
                                [],
                                new[] { sourceAddressPersistentLocalId }
                            )
                        },
                        command.Readdresses.SelectMany(x => x.Value).Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void WithSourceAddressNotAttachedAndDestinationNotAttached_ThenOnlyAttach()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(buildingUnitPersistentLocalId, sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(2)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingBuildingUnitsAddressesWereReaddressed(
                        command.BuildingPersistentLocalId,
                        new[]
                        {
                            new BuildingUnitAddressesWereReaddressed(
                                buildingUnitPersistentLocalId,
                                new[] { destinationAddressPersistentLocalId },
                                []
                            )
                        },
                        command.Readdresses.SelectMany(x => x.Value).Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void WithSourceAddressNotAttachedAndDestinationAddressAlreadyAttached_ThenNothing()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(buildingUnitPersistentLocalId, sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(2)
                    .WithAddress(destinationAddressPersistentLocalId)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithTwoReaddressesAndAddressIsBothSourceAndDestination_ThenOneAttachAndOneDetach()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstAddressPersistentLocalId = new AddressPersistentLocalId(3);
            var secondAddressPersistentLocalId = new AddressPersistentLocalId(5);

            var command = new ReaddressAddressesBuilder(Fixture)
                .WithReaddress(buildingUnitPersistentLocalId, sourceAddressPersistentLocalId, firstAddressPersistentLocalId)
                .WithReaddress(buildingUnitPersistentLocalId, secondAddressPersistentLocalId, sourceAddressPersistentLocalId)
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(secondAddressPersistentLocalId)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingBuildingUnitsAddressesWereReaddressed(
                        command.BuildingPersistentLocalId,
                        new[]
                        {
                            new BuildingUnitAddressesWereReaddressed(
                                buildingUnitPersistentLocalId,
                                new[] { firstAddressPersistentLocalId },
                                new[] { secondAddressPersistentLocalId }
                            )
                        },
                        command.Readdresses.SelectMany(x => x.Value).Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .WithAddress(otherAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingBuildingUnitsAddressesWereReaddressed(
                Fixture.Create<BuildingPersistentLocalId>(),
                new[]
                {
                    new BuildingUnitAddressesWereReaddressed(
                        buildingUnitPersistentLocalId,
                        new[] { destinationAddressPersistentLocalId },
                        new[] { sourceAddressPersistentLocalId }
                    )
                },
                new[] { new AddressRegistryReaddress(new ReaddressData(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)) }
            );
            @event.SetFixtureProvenance(Fixture);

            // Act
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated, @event });

            // Assert
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().HaveCount(2);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().Contain(destinationAddressPersistentLocalId);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().NotContain(sourceAddressPersistentLocalId);
            sut.BuildingUnits.First().LastEventHash.Should().Be(@event.GetHash());
            sut.LastEventHash.Should().NotBe(@event.GetHash());
        }
    }
}

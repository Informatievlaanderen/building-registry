namespace BuildingRegistry.Tests.AggregateTests.WhenReaddressingAddresses
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        //[Fact]
        //public void ThenBuildingUnitAddressAttachmentWasReplacedBecauseAddressWasReaddressed()
        //{
        //    var command = Fixture.Create<ReaddressAddresses>();

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
        //        .WithBuildingUnit(
        //            BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
        //            command.BuildingUnitPersistentLocalId,
        //            attachedAddresses: new List<AddressPersistentLocalId>(){ command.PreviousAddressPersistentLocalId },
        //            isRemoved: false)
        //        .Build();

        //    var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
        //    consumerAddress.AddAddress(command.PreviousAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);
        //    consumerAddress.AddAddress(command.NewAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);

        //    var buildingUnit = buildingWasMigrated.BuildingUnits.First();
        //    Assert(new Scenario()
        //        .Given(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            buildingWasMigrated)
        //        .When(command)
        //        .Then(new Fact(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //           new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
        //               new BuildingPersistentLocalId(buildingWasMigrated.BuildingPersistentLocalId),
        //               new BuildingUnitPersistentLocalId(buildingUnit.BuildingUnitPersistentLocalId),
        //               new AddressPersistentLocalId(command.PreviousAddressPersistentLocalId),
        //               new AddressPersistentLocalId(command.NewAddressPersistentLocalId)
        //               ))));
        //}

        //[Fact]
        //public void WithAlreadyReplacedAttachment_ThenBuildingUnitAddressAttachmentWasReplacedBecauseAddressWasReaddressed()
        //{
        //    var command = Fixture.Create<ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed>();

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
        //        .WithBuildingUnit(
        //            BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
        //            command.BuildingUnitPersistentLocalId,
        //            attachedAddresses: new List<AddressPersistentLocalId>(){ command.NewAddressPersistentLocalId },
        //            isRemoved: false)
        //        .Build();

        //    var consumerAddress = Container.Resolve<FakeConsumerAddressContext>();
        //    consumerAddress.AddAddress(command.PreviousAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);
        //    consumerAddress.AddAddress(command.NewAddressPersistentLocalId, Consumer.Address.AddressStatus.Current, isRemoved: false);


        //    var buildingUnit = buildingWasMigrated.BuildingUnits.First();
        //    Assert(new Scenario()
        //        .Given(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            buildingWasMigrated)
        //        .When(command)
        //        .ThenNone());
        //}

        [Fact]
        public void WithSourceAddressAttachedAndDestinationAddressNotAttached_ThenAttachAndDetach()
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
                    .WithAddress(sourceAddressPersistentLocalId)
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
                                new[] { sourceAddressPersistentLocalId }
                            )
                        },
                        command.Readdresses
                            .SelectMany(x => x.Value)
                            .Select(x => new AddressRegistryReaddress(x)).ToList()
                    )
                ));
        }

        //[Fact]
        //public void WithSourceAddressAttachedAndDestinationAddressAlreadyAttached_ThenOnlyDetach()
        //{
        //    var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
        //    var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

        //    var command = new ReaddressAddressesBuilder(Fixture)
        //        .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
        //        .Build();

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithStatus(ParcelStatus.Realized)
        //        .WithAddress(2)
        //        .WithAddress(sourceAddressPersistentLocalId)
        //        .WithAddress(destinationAddressPersistentLocalId)
        //        .Build();

        //    Assert(new Scenario()
        //        .Given(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            buildingWasMigrated)
        //        .When(command)
        //        .Then(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            new BuildingBuildingUnitsAddressesWereReaddressed(
        //                command.BuildingPersistentLocalId,
        //                new VbrCaPaKey(buildingWasMigrated.CaPaKey),
        //                Array.Empty<AddressPersistentLocalId>(),
        //                new[] { sourceAddressPersistentLocalId },
        //                command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
        //            )
        //        ));
        //}

        //[Fact]
        //public void WithSourceAddressNotAttachedAndDestinationNotAttached_ThenOnlyAttach()
        //{
        //    var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
        //    var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

        //    var command = new ReaddressAddressesBuilder(Fixture)
        //        .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
        //        .Build();

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithStatus(ParcelStatus.Realized)
        //        .WithAddress(2)
        //        .Build();

        //    Assert(new Scenario()
        //        .Given(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            buildingWasMigrated)
        //        .When(command)
        //        .Then(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            new BuildingBuildingUnitsAddressesWereReaddressed(
        //                command.BuildingPersistentLocalId,
        //                new VbrCaPaKey(buildingWasMigrated.CaPaKey),
        //                new[] { destinationAddressPersistentLocalId },
        //                Array.Empty<AddressPersistentLocalId>(),
        //                command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
        //            )
        //        ));
        //}

        //[Fact]
        //public void WithSourceAddressNotAttachedAndDestinationAddressAlreadyAttached_ThenNothing()
        //{
        //    var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
        //    var destinationAddressPersistentLocalId = new AddressPersistentLocalId(3);

        //    var command = new ReaddressAddressesBuilder(Fixture)
        //        .WithReaddress(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)
        //        .Build();

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithStatus(ParcelStatus.Realized)
        //        .WithAddress(2)
        //        .WithAddress(destinationAddressPersistentLocalId)
        //        .Build();

        //    Assert(new Scenario()
        //        .Given(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            buildingWasMigrated)
        //        .When(command)
        //        .ThenNone());
        //}

        //[Fact]
        //public void WithTwoReaddressesAndAddressIsBothSourceAndDestination_ThenOneAttachAndOneDetach()
        //{
        //    var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
        //    var firstAddressPersistentLocalId = new AddressPersistentLocalId(3);
        //    var secondAddressPersistentLocalId = new AddressPersistentLocalId(5);

        //    var command = new ReaddressAddressesBuilder(Fixture)
        //        .WithReaddress(sourceAddressPersistentLocalId, firstAddressPersistentLocalId)
        //        .WithReaddress(secondAddressPersistentLocalId, sourceAddressPersistentLocalId)
        //        .Build();

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithStatus(ParcelStatus.Realized)
        //        .WithAddress(secondAddressPersistentLocalId)
        //        .WithAddress(sourceAddressPersistentLocalId)
        //        .Build();

        //    Assert(new Scenario()
        //        .Given(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            buildingWasMigrated)
        //        .When(command)
        //        .Then(
        //            new BuildingStreamId(command.BuildingPersistentLocalId),
        //            new BuildingBuildingUnitsAddressesWereReaddressed(
        //                command.BuildingPersistentLocalId,
        //                new VbrCaPaKey(buildingWasMigrated.CaPaKey),
        //                new[] { firstAddressPersistentLocalId },
        //                new[] { secondAddressPersistentLocalId },
        //                command.Readdresses.Select(x => new AddressRegistryReaddress(x)).ToList()
        //            )
        //        ));
        //}

        //[Fact]
        //public void StateCheck()
        //{
        //    var sourceAddressPersistentLocalId = new AddressPersistentLocalId(1);
        //    var destinationAddressPersistentLocalId = new AddressPersistentLocalId(2);
        //    var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

        //    var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
        //        .WithStatus(ParcelStatus.Realized)
        //        .WithAddress(sourceAddressPersistentLocalId)
        //        .WithAddress(otherAddressPersistentLocalId)
        //        .Build();

        //    var @event = new BuildingBuildingUnitsAddressesWereReaddressed(
        //        Fixture.Create<ParcelId>(),
        //        Fixture.Create<VbrCaPaKey>(),
        //        new[] { destinationAddressPersistentLocalId },
        //        new[] { sourceAddressPersistentLocalId },
        //        new[] { new AddressRegistryReaddress(new ReaddressData(sourceAddressPersistentLocalId, destinationAddressPersistentLocalId)) }
        //    );
        //    @event.SetFixtureProvenance(Fixture);

        //    // Act
        //    var sut = new ParcelFactory(NoSnapshotStrategy.Instance, Container.Resolve<IAddresses>()).Create();
        //    sut.Initialize(new List<object> { buildingWasMigrated, @event });

        //    // Assert
        //    sut.AddressPersistentLocalIds.Should().HaveCount(2);
        //    sut.AddressPersistentLocalIds.Should().Contain(destinationAddressPersistentLocalId);
        //    sut.AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
        //    sut.AddressPersistentLocalIds.Should().NotContain(sourceAddressPersistentLocalId);
        //    sut.LastEventHash.Should().Be(@event.GetHash());
        //}
    }
}

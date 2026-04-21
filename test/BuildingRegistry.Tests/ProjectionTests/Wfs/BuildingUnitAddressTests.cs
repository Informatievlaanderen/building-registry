namespace BuildingRegistry.Tests.ProjectionTests.Wfs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Projections.Wfs.BuildingUnitAddress;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public sealed class BuildingUnitAddressTests : BuildingWfsProjectionTest<BuildingUnitAddressProjections>
    {
        private readonly Fixture _fixture = new();

        public BuildingUnitAddressTests()
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPolygon());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
        }

        [Fact]
        public async Task WhenBuildingWasMigrated()
        {
            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(
                    new Envelope(
                        buildingWasMigrated,
                        new Dictionary<string, object>
                        {
                            { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                        })))
                .Then(ct =>
                {
                    var addresses = ct.BuildingUnitAddresses
                        .Where(x => buildingWasMigrated.BuildingUnits
                            .Select(y => y.BuildingUnitPersistentLocalId)
                            .Contains(x.BuildingUnitPersistentLocalId))
                        .ToList();

                    addresses.Count.Should().Be(
                        buildingWasMigrated.BuildingUnits.Sum(x => x.AddressPersistentLocalIds.Count));
                    addresses.Should().OnlyContain(x => x.Count == 1);

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasAttachedV2()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            await Sut
                .Given(new Envelope<BuildingUnitAddressWasAttachedV2>(
                    new Envelope(
                        @event,
                        new Dictionary<string, object>
                        {
                            { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                        })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        @event.BuildingUnitPersistentLocalId,
                        @event.AddressPersistentLocalId);

                    address.Should().NotBeNull();
                    address!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedV2()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(addressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasDetachedV2(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasDetachedV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRejected()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = CreateBuildingWasMigratedWithSingleAddress(
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);

            var @event = new BuildingUnitAddressWasDetachedBecauseAddressWasRejected(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRetired()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = CreateBuildingWasMigratedWithSingleAddress(
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);

            var @event = new BuildingUnitAddressWasDetachedBecauseAddressWasRetired(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRemoved()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = CreateBuildingWasMigratedWithSingleAddress(
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);

            var @event = new BuildingUnitAddressWasDetachedBecauseAddressWasRemoved(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenPreviousAddressCountOne_WhenReaddressed_ThenRelationIsReplaced()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var previousAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var newAddressPersistentLocalId = new AddressPersistentLocalId(previousAddressPersistentLocalId + 1);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                previousAddressPersistentLocalId,
                newAddressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var previousAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)previousAddressPersistentLocalId);
                    var newAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)newAddressPersistentLocalId);

                    previousAddress.Should().BeNull();
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenExistingNewAddress_WhenReaddressed_ThenNewAddressCountIsIncremented()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var previousAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var newAddressPersistentLocalId = new AddressPersistentLocalId(previousAddressPersistentLocalId + 1);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .WithAddress(newAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                previousAddressPersistentLocalId,
                newAddressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var previousAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)previousAddressPersistentLocalId);
                    var newAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)newAddressPersistentLocalId);

                    previousAddress.Should().BeNull();
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(2);
                });
        }

        [Fact]
        public async Task GivenPreviousAddressCountTwo_WhenReaddressed_ThenPreviousAddressCountIsDecremented()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var previousAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var secondSourceAddressPersistentLocalId = new AddressPersistentLocalId(previousAddressPersistentLocalId + 100);
            var destinationAddressPersistentLocalId = new AddressPersistentLocalId(previousAddressPersistentLocalId + 101);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .Build())
                .Build();

            var eventToIncreasePreviousAddressCount = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                secondSourceAddressPersistentLocalId,
                previousAddressPersistentLocalId);
            ((ISetProvenance)eventToIncreasePreviousAddressCount).SetProvenance(_fixture.Create<Provenance>());

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                previousAddressPersistentLocalId,
                destinationAddressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            eventToIncreasePreviousAddressCount,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, eventToIncreasePreviousAddressCount.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var previousAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)previousAddressPersistentLocalId);
                    var destinationAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)destinationAddressPersistentLocalId);

                    previousAddress.Should().NotBeNull();
                    previousAddress!.Count.Should().Be(1);
                    destinationAddress.Should().NotBeNull();
                    destinationAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenReaddressToSameAddress_WhenReplaced_ThenDeletedEntryIsRevived()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(addressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                addressPersistentLocalId,
                addressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().NotBeNull();
                    address!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenDetachedAndAttachedSameAddressInBatch_WhenReaddressed_ThenDeletedEntryIsRevived()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(addressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingBuildingUnitsAddressesWereReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                [
                    new BuildingUnitAddressesWereReaddressed(
                        buildingUnitPersistentLocalId,
                        [addressPersistentLocalId],
                        [addressPersistentLocalId])
                ],
                []);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingBuildingUnitsAddressesWereReaddressed>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().NotBeNull();
                    address!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var previousAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var newAddressPersistentLocalId = new AddressPersistentLocalId(previousAddressPersistentLocalId + 1);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                newAddressPersistentLocalId: newAddressPersistentLocalId,
                previousAddressPersistentLocalId: previousAddressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var previousAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)previousAddressPersistentLocalId);
                    var newAddress = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)newAddressPersistentLocalId);

                    previousAddress.Should().BeNull();
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenMunicipalityMergerToSameAddress_WhenReplaced_ThenDeletedEntryIsRevived()
        {
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = CreateBuildingWasMigratedWithSingleAddress(
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);

            var @event = new BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                newAddressPersistentLocalId: addressPersistentLocalId,
                previousAddressPersistentLocalId: addressPersistentLocalId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(
                            buildingWasMigrated,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
                            })),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
                            })))
                .Then(async ct =>
                {
                    var address = await ct.BuildingUnitAddresses.FindAsync(
                        (int)buildingUnitPersistentLocalId,
                        (int)addressPersistentLocalId);

                    address.Should().NotBeNull();
                    address!.Count.Should().Be(1);
                });
        }

        private BuildingWasMigrated CreateBuildingWasMigratedWithSingleAddress(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
            => new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(addressPersistentLocalId)
                    .Build())
                .Build();

        protected override BuildingUnitAddressProjections CreateProjection() => new();
    }
}

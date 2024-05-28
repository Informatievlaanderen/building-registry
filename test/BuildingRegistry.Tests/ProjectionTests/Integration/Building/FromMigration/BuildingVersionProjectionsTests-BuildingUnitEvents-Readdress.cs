// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
// ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery
namespace BuildingRegistry.Tests.ProjectionTests.Integration.Building.FromMigration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;

    public sealed partial class BuildingVersionProjectionsTests
    {
        [Fact]
        public async Task GivenOnlyPreviousBuildingUnitAddressRelationExistsWithCountOne_ThenRelationIsReplaced()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var position = _fixture.Create<long>();
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithAddress(addressPersistentLocalId)
                    .Build())
                .Build();

            var buildingUnitAddressWasReplacedBecauseAddressWasReaddressed = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId + 1));
            ((ISetProvenance)buildingUnitAddressWasReplacedBecauseAddressWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasReplacedBecauseAddressWasReaddressedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, buildingWasMigratedMetadata)),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            buildingUnitAddressWasReplacedBecauseAddressWasReaddressed,
                            buildingUnitAddressWasReplacedBecauseAddressWasReaddressedMetadata))
                )
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersionsFromMigration.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    var previousAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.PreviousAddressPersistentLocalId);
                    previousAddress.Should().BeNull();

                    var newAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.NewAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenOnlyPreviousBuildingUnitAddressRelationExistsWithCountTwo_ThenCountIsDecrementedByOne()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var position = _fixture.Create<long>();
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithAddress(addressPersistentLocalId)
                    .Build())
                .Build();

            var eventToAddPreviousRelationASecondTime = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                new AddressPersistentLocalId(addressPersistentLocalId + 100),
                new AddressPersistentLocalId(addressPersistentLocalId));
            ((ISetProvenance)eventToAddPreviousRelationASecondTime).SetProvenance(_fixture.Create<Provenance>());

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                new AddressPersistentLocalId(addressPersistentLocalId),
                new AddressPersistentLocalId(addressPersistentLocalId + 101));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventToAddPreviousRelationASecondTimeMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(buildingWasMigrated, buildingWasMigratedMetadata)),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(eventToAddPreviousRelationASecondTime, eventToAddPreviousRelationASecondTimeMetadata)),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(@event, eventMetadata))
                )
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersionsFromMigration.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    var previousAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == @event.PreviousAddressPersistentLocalId);
                    previousAddress.Should().NotBeNull();
                    previousAddress!.Count.Should().Be(1);

                    var newAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == @event.NewAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenNewBuildingUnitAddressRelationExists_ThenCountIsIncrementedByOne()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var position = _fixture.Create<long>();
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var previousAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var newAddressPersistentLocalId = new BuildingPersistentLocalId(previousAddressPersistentLocalId + 1);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithAddress(previousAddressPersistentLocalId)
                    .WithAddress(newAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                new AddressPersistentLocalId(previousAddressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var eventMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(
                        new Envelope(buildingWasMigrated, buildingWasMigratedMetadata)),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(@event, eventMetadata))
                )
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersionsFromMigration.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(@event.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    var previousAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == @event.PreviousAddressPersistentLocalId);
                    previousAddress.Should().BeNull();

                    var newAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == @event.NewAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();
                    newAddress!.Count.Should().Be(2);
                });
        }

        [Fact]
        public async Task WhenBuildingBuildingUnitsAddressesWereReaddressed()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var position = _fixture.Create<long>();
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var sourceAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var destinationAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .Build()
                ).Build();

            var buildingBuildingUnitsAddressesWereReaddressed = new BuildingBuildingUnitsAddressesWereReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                [
                    new BuildingUnitAddressesWereReaddressed(
                        _fixture.Create<BuildingUnitPersistentLocalId>(),
                        [new AddressPersistentLocalId(destinationAddressPersistentLocalId)],
                        [new AddressPersistentLocalId(sourceAddressPersistentLocalId)]
                    )
                ],
                []);
            ((ISetProvenance)buildingBuildingUnitsAddressesWereReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingBuildingUnitsAddressesWereReaddressedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingBuildingUnitsAddressesWereReaddressed.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, buildingWasMigratedMetadata)),
                    new Envelope<BuildingBuildingUnitsAddressesWereReaddressed>(
                        new Envelope(
                            buildingBuildingUnitsAddressesWereReaddressed,
                            buildingBuildingUnitsAddressesWereReaddressedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersionsFromMigration.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingBuildingUnitsAddressesWereReaddressed.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    var destinationAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
                    destinationAddress.Should().NotBeNull();

                    var sourceAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == sourceAddressPersistentLocalId);
                    sourceAddress.Should().BeNull();
                });
        }
    }
}

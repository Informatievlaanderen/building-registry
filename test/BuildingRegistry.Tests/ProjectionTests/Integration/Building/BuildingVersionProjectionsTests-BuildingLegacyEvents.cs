// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
// ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery
#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Tests.ProjectionTests.Integration.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Legacy.Events;
    using FluentAssertions;
    using Moq;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;

    public partial class BuildingVersionProjectionsTests
    {
        [Fact]
        public async Task WhenBuildingWasRegistered()
        {
            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingPersistentLocalId = (int) _fixture.Create<BuildingPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync(buildingPersistentLocalId);

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, metadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalId);
                    buildingVersion.Namespace.Should().Be(BuildingNamespace);
                    buildingVersion.PuriId.Should().Be($"{BuildingNamespace}/{buildingPersistentLocalId}");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRegistered.Provenance.Timestamp);
                    buildingVersion.CreatedOnTimestamp.Should().Be(buildingWasRegistered.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasRegistered.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingPersistentLocalIdWasAssigned()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingPersistentLocalIdWasAssigned = _fixture.Create<BuildingPersistentLocalIdWasAssigned>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId();
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingPersistentLocalIdWasAssignedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingPersistentLocalIdWasAssigned>(new Envelope(buildingPersistentLocalIdWasAssigned, buildingPersistentLocalIdWasAssignedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalIdWasAssigned.PersistentLocalId);
                    buildingVersion.VersionTimestamp.Should().Be(buildingPersistentLocalIdWasAssigned.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingPersistentLocalIdWasAssigned.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    foreach (var buildingUnitVersion in buildingVersion.BuildingUnits)
                    {
                        buildingUnitVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalIdWasAssigned.PersistentLocalId);
                        buildingUnitVersion.VersionTimestamp.Should().Be(buildingPersistentLocalIdWasAssigned.Provenance.Timestamp);
                        buildingUnitVersion.Type.Should().Be("EventName");
                    }
                });
        }

        [Fact]
        public async Task WhenBuildingWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasRemoved = new BuildingWasRemoved(
                _fixture.Create<BuildingId>(),
                new []
                {
                    new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId)
                });
            ((ISetProvenance)buildingWasRemoved).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId();
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasRemoved>(new Envelope(buildingWasRemoved, buildingWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.IsRemoved.Should().BeTrue();
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRemoved.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasRemoved.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    foreach (var buildingUnitVersion in buildingVersion.BuildingUnits)
                    {
                        buildingUnitVersion.IsRemoved.Should().BeTrue();
                        buildingUnitVersion.Addresses.Should().BeEmpty();
                        buildingUnitVersion.VersionTimestamp.Should().Be(buildingWasRemoved.Provenance.Timestamp);
                        buildingUnitVersion.Type.Should().Be("EventName");
                    }
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstruction()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstruction>();

            AddBuildingPersistentLocalId();

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingBecameUnderConstructionMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingBecameUnderConstruction>(new Envelope(buildingBecameUnderConstruction, buildingBecameUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("UnderConstruction");
                    buildingVersion.OsloStatus.Should().Be("InAanbouw");
                    buildingVersion.VersionTimestamp.Should().Be(buildingBecameUnderConstruction.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingBecameUnderConstruction.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToUnderConstruction()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToUnderConstruction = _fixture.Create<BuildingWasCorrectedToUnderConstruction>();

            AddBuildingPersistentLocalId();

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToUnderConstructionMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToUnderConstruction>(new Envelope(buildingWasCorrectedToUnderConstruction, buildingWasCorrectedToUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("UnderConstruction");
                    buildingVersion.OsloStatus.Should().Be("InAanbouw");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToUnderConstruction.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasCorrectedToUnderConstruction.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var plannedBuildingUnitPersistentLocalId = _fixture.Create<int>();
            var realizedBuildingUnitPersistentLocalId = plannedBuildingUnitPersistentLocalId + 1;

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var plannedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var realizedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasNotRealized = new BuildingWasNotRealized(
                _fixture.Create<BuildingId>(),
                new []{ new BuildingUnitId(realizedBuildingUnitWasAdded.BuildingUnitId) },
                new []{ new BuildingUnitId(plannedBuildingUnitWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasNotRealized).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(plannedBuildingUnitWasAdded.BuildingUnitId, plannedBuildingUnitPersistentLocalId);
            AddBuildingUnitPersistentLocalId(realizedBuildingUnitWasAdded.BuildingUnitId, realizedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(plannedBuildingUnitWasAdded.AddressId, plannedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(realizedBuildingUnitWasAdded.AddressId, realizedBuildingUnitPersistentLocalId);

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var plannedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var realizedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(plannedBuildingUnitWasAdded, plannedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(realizedBuildingUnitWasAdded, realizedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasNotRealized>(new Envelope(buildingWasNotRealized, buildingWasNotRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("NotRealized");
                    buildingVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var notRealizedBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == plannedBuildingUnitWasAdded.BuildingUnitId);
                    notRealizedBuildingUnit.Should().NotBeNull();
                    notRealizedBuildingUnit!.Status.Should().Be("NotRealized");
                    notRealizedBuildingUnit.OsloStatus.Should().Be("NietGerealiseerd");
                    notRealizedBuildingUnit.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnit.VersionTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                    notRealizedBuildingUnit.Type.Should().Be("EventName");

                    var retiredBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == realizedBuildingUnitWasAdded.BuildingUnitId);
                    retiredBuildingUnit.Should().NotBeNull();
                    retiredBuildingUnit!.Status.Should().Be("Retired");
                    retiredBuildingUnit.OsloStatus.Should().Be("Gehistoreerd");
                    retiredBuildingUnit.Addresses.Should().BeEmpty();
                    retiredBuildingUnit.VersionTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                    retiredBuildingUnit.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var plannedBuildingUnitPersistentLocalId = _fixture.Create<int>();
            var realizedBuildingUnitPersistentLocalId = plannedBuildingUnitPersistentLocalId + 1;

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var plannedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var realizedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasCorrectedToNotRealized = new BuildingWasCorrectedToNotRealized(
                _fixture.Create<BuildingId>(),
                new []{ new BuildingUnitId(realizedBuildingUnitWasAdded.BuildingUnitId) },
                new []{ new BuildingUnitId(plannedBuildingUnitWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasCorrectedToNotRealized).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(plannedBuildingUnitWasAdded.BuildingUnitId, plannedBuildingUnitPersistentLocalId);
            AddBuildingUnitPersistentLocalId(realizedBuildingUnitWasAdded.BuildingUnitId, realizedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(plannedBuildingUnitWasAdded.AddressId, plannedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(realizedBuildingUnitWasAdded.AddressId, realizedBuildingUnitPersistentLocalId);

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var plannedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var realizedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(plannedBuildingUnitWasAdded, plannedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(realizedBuildingUnitWasAdded, realizedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasCorrectedToNotRealized>(new Envelope(buildingWasCorrectedToNotRealized, buildingWasCorrectedToNotRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("NotRealized");
                    buildingVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var notRealizedBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == plannedBuildingUnitWasAdded.BuildingUnitId);
                    notRealizedBuildingUnit.Should().NotBeNull();
                    notRealizedBuildingUnit!.Status.Should().Be("NotRealized");
                    notRealizedBuildingUnit.OsloStatus.Should().Be("NietGerealiseerd");
                    notRealizedBuildingUnit.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnit.VersionTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);
                    notRealizedBuildingUnit.Type.Should().Be("EventName");

                    var retiredBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == realizedBuildingUnitWasAdded.BuildingUnitId);
                    retiredBuildingUnit.Should().NotBeNull();
                    retiredBuildingUnit!.Status.Should().Be("Retired");
                    retiredBuildingUnit.OsloStatus.Should().Be("Gehistoreerd");
                    retiredBuildingUnit.Addresses.Should().BeEmpty();
                    retiredBuildingUnit.VersionTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);
                    retiredBuildingUnit.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlanned()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasPlanned = _fixture.Create<BuildingWasPlanned>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasPlanned>(new Envelope(buildingWasPlanned, buildingWasPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasPlanned.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasPlanned.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToPlanned = _fixture.Create<BuildingWasCorrectedToPlanned>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToPlanned>(new Envelope(buildingWasCorrectedToPlanned, buildingWasCorrectedToPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToPlanned.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasCorrectedToPlanned.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasRealized = _fixture.Create<BuildingWasRealized>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasRealized>(new Envelope(buildingWasRealized, buildingWasRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Realized");
                    buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRealized.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasRealized.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToRealized = _fixture.Create<BuildingWasCorrectedToRealized>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToRealized>(new Envelope(buildingWasCorrectedToRealized, buildingWasCorrectedToRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Realized");
                    buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToRealized.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasCorrectedToRealized.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasRetired()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var plannedBuildingUnitPersistentLocalId = _fixture.Create<int>();
            var realizedBuildingUnitPersistentLocalId = plannedBuildingUnitPersistentLocalId + 1;

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var plannedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var realizedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasRetired = new BuildingWasRetired(
                _fixture.Create<BuildingId>(),
                new []{ new BuildingUnitId(realizedBuildingUnitWasAdded.BuildingUnitId) },
                new []{ new BuildingUnitId(plannedBuildingUnitWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasRetired).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(plannedBuildingUnitWasAdded.BuildingUnitId, plannedBuildingUnitPersistentLocalId);
            AddBuildingUnitPersistentLocalId(realizedBuildingUnitWasAdded.BuildingUnitId, realizedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(plannedBuildingUnitWasAdded.AddressId, plannedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(realizedBuildingUnitWasAdded.AddressId, realizedBuildingUnitPersistentLocalId);

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var plannedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var realizedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(plannedBuildingUnitWasAdded, plannedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(realizedBuildingUnitWasAdded, realizedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasRetired>(new Envelope(buildingWasRetired, buildingWasRetiredMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Retired");
                    buildingVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var notRealizedBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == plannedBuildingUnitWasAdded.BuildingUnitId);
                    notRealizedBuildingUnit.Should().NotBeNull();
                    notRealizedBuildingUnit!.Status.Should().Be("NotRealized");
                    notRealizedBuildingUnit.OsloStatus.Should().Be("NietGerealiseerd");
                    notRealizedBuildingUnit.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnit.VersionTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);
                    notRealizedBuildingUnit.Type.Should().Be("EventName");

                    var retiredBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == realizedBuildingUnitWasAdded.BuildingUnitId);
                    retiredBuildingUnit.Should().NotBeNull();
                    retiredBuildingUnit!.Status.Should().Be("Retired");
                    retiredBuildingUnit.OsloStatus.Should().Be("Gehistoreerd");
                    retiredBuildingUnit.Addresses.Should().BeEmpty();
                    retiredBuildingUnit.VersionTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);
                    retiredBuildingUnit.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToRetired()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var plannedBuildingUnitPersistentLocalId = _fixture.Create<int>();
            var realizedBuildingUnitPersistentLocalId = plannedBuildingUnitPersistentLocalId + 1;

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var plannedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var realizedBuildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasCorrectedToRetired = new BuildingWasCorrectedToRetired(
                _fixture.Create<BuildingId>(),
                new []{ new BuildingUnitId(realizedBuildingUnitWasAdded.BuildingUnitId) },
                new []{ new BuildingUnitId(plannedBuildingUnitWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasCorrectedToRetired).SetProvenance(_fixture.Create<Provenance>());

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId(plannedBuildingUnitWasAdded.BuildingUnitId, plannedBuildingUnitPersistentLocalId);
            AddBuildingUnitPersistentLocalId(realizedBuildingUnitWasAdded.BuildingUnitId, realizedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(plannedBuildingUnitWasAdded.AddressId, plannedBuildingUnitPersistentLocalId);
            AddAddressPersistentLocalId(realizedBuildingUnitWasAdded.AddressId, realizedBuildingUnitPersistentLocalId);

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var plannedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var realizedBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(plannedBuildingUnitWasAdded, plannedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(realizedBuildingUnitWasAdded, realizedBuildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasCorrectedToRetired>(new Envelope(buildingWasCorrectedToRetired, buildingWasCorrectedToRetiredMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Retired");
                    buildingVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var notRealizedBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == plannedBuildingUnitWasAdded.BuildingUnitId);
                    notRealizedBuildingUnit.Should().NotBeNull();
                    notRealizedBuildingUnit!.Status.Should().Be("NotRealized");
                    notRealizedBuildingUnit.OsloStatus.Should().Be("NietGerealiseerd");
                    notRealizedBuildingUnit.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnit.VersionTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);
                    notRealizedBuildingUnit.Type.Should().Be("EventName");

                    var retiredBuildingUnit = buildingVersion.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitId == realizedBuildingUnitWasAdded.BuildingUnitId);
                    retiredBuildingUnit.Should().NotBeNull();
                    retiredBuildingUnit!.Status.Should().Be("Retired");
                    retiredBuildingUnit.OsloStatus.Should().Be("Gehistoreerd");
                    retiredBuildingUnit.Addresses.Should().BeEmpty();
                    retiredBuildingUnit.VersionTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);
                    retiredBuildingUnit.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingStatusWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingStatusWasRemoved = _fixture.Create<BuildingStatusWasRemoved>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingStatusWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingStatusWasRemoved>(new Envelope(buildingStatusWasRemoved, buildingStatusWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().BeNull();
                    buildingVersion.OsloStatus.Should().BeNull();
                    buildingVersion.VersionTimestamp.Should().Be(buildingStatusWasRemoved.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingStatusWasRemoved.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingStatusWasCorrectedToRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingStatusWasCorrectedToRemoved = _fixture.Create<BuildingStatusWasCorrectedToRemoved>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingStatusWasCorrectedToRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingStatusWasCorrectedToRemoved>(new Envelope(buildingStatusWasCorrectedToRemoved, buildingStatusWasCorrectedToRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().BeNull();
                    buildingVersion.OsloStatus.Should().BeNull();
                    buildingVersion.VersionTimestamp.Should().Be(buildingStatusWasCorrectedToRemoved.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingStatusWasCorrectedToRemoved.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasuredByGrb()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasMeasuredByGrb = _fixture.Create<BuildingWasMeasuredByGrb>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasMeasuredByGrbMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasMeasuredByGrb>(new Envelope(buildingWasMeasuredByGrb, buildingWasMeasuredByGrbMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasMeasuredByGrb.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasMeasuredByGrb.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasMeasuredByGrb.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingGeometryWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPositionWasDerivedFromObject = _fixture.Create<BuildingUnitPositionWasDerivedFromObject>();
            var buildingWasMeasuredByGrb = _fixture.Create<BuildingWasMeasuredByGrb>();
            var buildingGeometryWasRemoved = _fixture.Create<BuildingGeometryWasRemoved>();

            AddBuildingPersistentLocalId();
            AddBuildingUnitPersistentLocalId();
            AddAddressPersistentLocalId(buildingUnitWasAdded.AddressId);;

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitPositionWasDerivedFromObjectMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasMeasuredByGrbMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingStatusWasCorrectedToRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPositionWasDerivedFromObject>(new Envelope(buildingUnitPositionWasDerivedFromObject, buildingUnitPositionWasDerivedFromObjectMetadata)),
                    new Envelope<BuildingWasMeasuredByGrb>(new Envelope(buildingWasMeasuredByGrb, buildingWasMeasuredByGrbMetadata)),
                    new Envelope<BuildingGeometryWasRemoved>(new Envelope(buildingGeometryWasRemoved, buildingStatusWasCorrectedToRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeNull();
                    buildingVersion.GeometryMethod.Should().BeNull();
                    buildingVersion.OsloGeometryMethod.Should().BeNull();
                    buildingVersion.VersionTimestamp.Should().Be(buildingGeometryWasRemoved.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingGeometryWasRemoved.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    foreach (var buildingUnitVersion in buildingVersion.BuildingUnits)
                    {
                        buildingUnitVersion.Geometry.Should().BeNull();
                        buildingUnitVersion.GeometryMethod.Should().BeNull();
                        buildingUnitVersion.OsloGeometryMethod.Should().BeNull();
                        buildingUnitVersion.VersionTimestamp.Should().Be(buildingGeometryWasRemoved.Provenance.Timestamp);
                        buildingUnitVersion.Type.Should().Be("EventName");
                    }
                });
        }

        [Fact]
        public async Task WhenBuildingWasOutlined()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasOutlined = _fixture.Create<BuildingWasOutlined>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasOutlinedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasOutlined>(new Envelope(buildingWasOutlined, buildingWasOutlinedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasOutlined.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("Outlined");
                    buildingVersion.OsloGeometryMethod.Should().Be("Ingeschetst");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasOutlined.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasOutlined.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementByGrbWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingMeasurementByGrbWasCorrected = _fixture.Create<BuildingMeasurementByGrbWasCorrected>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingMeasurementByGrbWasCorrectedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingMeasurementByGrbWasCorrected>(new Envelope(buildingMeasurementByGrbWasCorrected, buildingMeasurementByGrbWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingMeasurementByGrbWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingMeasurementByGrbWasCorrected.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingMeasurementByGrbWasCorrected.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingOutlineWasCorrected = _fixture.Create<BuildingOutlineWasCorrected>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingOutlineWasCorrectedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingOutlineWasCorrected>(new Envelope(buildingOutlineWasCorrected, buildingOutlineWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingOutlineWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("Outlined");
                    buildingVersion.OsloGeometryMethod.Should().Be("Ingeschetst");
                    buildingVersion.VersionTimestamp.Should().Be(buildingOutlineWasCorrected.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingOutlineWasCorrected.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

         private void AddBuildingUnitPersistentLocalId(Guid buildingUnitId, int buildingUnitPersistentLocalId)
         {
             _persistentLocalIdFinder
                 .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitId))
                 .ReturnsAsync(buildingUnitPersistentLocalId);
         }

         private void AddBuildingUnitPersistentLocalId(int? buildingUnitPersistentLocalId = null)
         {
             var persistentLocalId = buildingUnitPersistentLocalId ?? (int) _fixture.Create<BuildingPersistentLocalId>();

             _persistentLocalIdFinder
                 .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                 .ReturnsAsync(persistentLocalId);
         }

         private void AddBuildingPersistentLocalId(int? buildingPersistentLocalId = null)
         {
             var persistentLocalId = buildingPersistentLocalId ?? (int) _fixture.Create<BuildingPersistentLocalId>();

             _persistentLocalIdFinder
                 .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>()))
                 .ReturnsAsync(persistentLocalId);
         }

         private void AddAddressPersistentLocalId(Guid addressId, int addressPersistentLocalId = 1)
         {
             _addresses
                 .Setup(x => x.GetAddressPersistentLocalId(addressId))
                 .ReturnsAsync(addressPersistentLocalId);
         }
    }
}

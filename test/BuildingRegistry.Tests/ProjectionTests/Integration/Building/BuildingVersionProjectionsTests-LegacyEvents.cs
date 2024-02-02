#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Tests.ProjectionTests.Integration.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Legacy.Events;
    using FluentAssertions;
    using Moq;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public partial class BuildingVersionProjectionsTests
    {
        [Fact]
        public async Task WhenBuildingWasRegistered()
        {
            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingPersistentLocalId = (int) _fixture.Create<BuildingPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingPersistentLocalId);

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
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
                });
        }

        [Fact]
        public async Task WhenBuildingPersistentLocalIdWasAssigned()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingPersistentLocalIdWasAssigned = _fixture.Create<BuildingPersistentLocalIdWasAssigned>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingPersistentLocalIdWasAssignedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingPersistentLocalIdWasAssigned>(new Envelope(buildingPersistentLocalIdWasAssigned, buildingPersistentLocalIdWasAssignedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalIdWasAssigned.PersistentLocalId);
                    buildingVersion.VersionTimestamp.Should().Be(buildingPersistentLocalIdWasAssigned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasRemoved = _fixture.Create<BuildingWasRemoved>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasRemoved>(new Envelope(buildingWasRemoved, buildingWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.IsRemoved.Should().BeTrue();
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameComplete()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingBecameComplete = _fixture.Create<BuildingBecameComplete>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingBecameCompleteMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingBecameComplete>(new Envelope(buildingBecameComplete, buildingBecameCompleteMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.VersionTimestamp.Should().Be(buildingBecameComplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameIncomplete()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingBecameIncomplete = _fixture.Create<BuildingBecameIncomplete>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingBecameIncompleteMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingBecameIncomplete>(new Envelope(buildingBecameIncomplete, buildingBecameIncompleteMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.VersionTimestamp.Should().Be(buildingBecameIncomplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstruction()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstruction>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingBecameUnderConstructionMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingBecameUnderConstruction>(new Envelope(buildingBecameUnderConstruction, buildingBecameUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("UnderConstruction");
                    buildingVersion.OsloStatus.Should().Be("InAanbouw");
                    buildingVersion.VersionTimestamp.Should().Be(buildingBecameUnderConstruction.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToUnderConstruction()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToUnderConstruction = _fixture.Create<BuildingWasCorrectedToUnderConstruction>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToUnderConstructionMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToUnderConstruction>(new Envelope(buildingWasCorrectedToUnderConstruction, buildingWasCorrectedToUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("UnderConstruction");
                    buildingVersion.OsloStatus.Should().Be("InAanbouw");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToUnderConstruction.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasNotRealized = _fixture.Create<BuildingWasNotRealized>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasNotRealized>(new Envelope(buildingWasNotRealized, buildingWasNotRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("NotRealized");
                    buildingVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToNotRealized = _fixture.Create<BuildingWasCorrectedToNotRealized>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToNotRealized>(new Envelope(buildingWasCorrectedToNotRealized, buildingWasCorrectedToNotRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("NotRealized");
                    buildingVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlanned()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasPlanned = _fixture.Create<BuildingWasPlanned>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasPlanned>(new Envelope(buildingWasPlanned, buildingWasPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToPlanned = _fixture.Create<BuildingWasCorrectedToPlanned>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToPlanned>(new Envelope(buildingWasCorrectedToPlanned, buildingWasCorrectedToPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasRealized = _fixture.Create<BuildingWasRealized>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasRealized>(new Envelope(buildingWasRealized, buildingWasRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Realized");
                    buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToRealized = _fixture.Create<BuildingWasCorrectedToRealized>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToRealized>(new Envelope(buildingWasCorrectedToRealized, buildingWasCorrectedToRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Realized");
                    buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRetired()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasRetired = _fixture.Create<BuildingWasRetired>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasRetired>(new Envelope(buildingWasRetired, buildingWasRetiredMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Retired");
                    buildingVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToRetired()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasCorrectedToRetired = _fixture.Create<BuildingWasCorrectedToRetired>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasCorrectedToRetired>(new Envelope(buildingWasCorrectedToRetired, buildingWasCorrectedToRetiredMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().Be("Retired");
                    buildingVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingStatusWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingStatusWasRemoved = _fixture.Create<BuildingStatusWasRemoved>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingStatusWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingStatusWasRemoved>(new Envelope(buildingStatusWasRemoved, buildingStatusWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().BeNull();
                    buildingVersion.OsloStatus.Should().BeNull();
                    buildingVersion.VersionTimestamp.Should().Be(buildingStatusWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingStatusWasCorrectedToRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingStatusWasCorrectedToRemoved = _fixture.Create<BuildingStatusWasCorrectedToRemoved>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingStatusWasCorrectedToRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingStatusWasCorrectedToRemoved>(new Envelope(buildingStatusWasCorrectedToRemoved, buildingStatusWasCorrectedToRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Status.Should().BeNull();
                    buildingVersion.OsloStatus.Should().BeNull();
                    buildingVersion.VersionTimestamp.Should().Be(buildingStatusWasCorrectedToRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasuredByGrb()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasMeasuredByGrb = _fixture.Create<BuildingWasMeasuredByGrb>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasMeasuredByGrbMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasMeasuredByGrb>(new Envelope(buildingWasMeasuredByGrb, buildingWasMeasuredByGrbMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasMeasuredByGrb.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasMeasuredByGrb.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingGeometryWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasMeasuredByGrb = _fixture.Create<BuildingWasMeasuredByGrb>();
            var buildingGeometryWasRemoved = _fixture.Create<BuildingGeometryWasRemoved>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasMeasuredByGrbMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingStatusWasCorrectedToRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasMeasuredByGrb>(new Envelope(buildingWasMeasuredByGrb, buildingWasMeasuredByGrbMetadata)),
                    new Envelope<BuildingGeometryWasRemoved>(new Envelope(buildingGeometryWasRemoved, buildingStatusWasCorrectedToRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 2);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeNull();
                    buildingVersion.GeometryMethod.Should().BeNull();
                    buildingVersion.OsloGeometryMethod.Should().BeNull();
                    buildingVersion.NisCode.Should().BeNull();
                    buildingVersion.VersionTimestamp.Should().Be(buildingGeometryWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasOutlined()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingWasOutlined = _fixture.Create<BuildingWasOutlined>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasOutlinedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingWasOutlined>(new Envelope(buildingWasOutlined, buildingWasOutlinedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasOutlined.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("Outlined");
                    buildingVersion.OsloGeometryMethod.Should().Be("Ingeschetst");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasOutlined.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementByGrbWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingMeasurementByGrbWasCorrected = _fixture.Create<BuildingMeasurementByGrbWasCorrected>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingMeasurementByGrbWasCorrectedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingMeasurementByGrbWasCorrected>(new Envelope(buildingMeasurementByGrbWasCorrected, buildingMeasurementByGrbWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingMeasurementByGrbWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingMeasurementByGrbWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingId());

            var buildingWasRegistered = _fixture.Create<BuildingWasRegistered>();
            var buildingOutlineWasCorrected = _fixture.Create<BuildingOutlineWasCorrected>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var position = _fixture.Create<long>();
            var buildingWasRegisteredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingOutlineWasCorrectedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasRegistered>(new Envelope(buildingWasRegistered, buildingWasRegisteredMetadata)),
                    new Envelope<BuildingOutlineWasCorrected>(new Envelope(buildingOutlineWasCorrected, buildingOutlineWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position + 1);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.BuildingId.Should().Be(buildingWasRegistered.BuildingId);
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingOutlineWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("Outlined");
                    buildingVersion.OsloGeometryMethod.Should().Be("Ingeschetst");
                    buildingVersion.VersionTimestamp.Should().Be(buildingOutlineWasCorrected.Provenance.Timestamp);
                });
        }
    }
}

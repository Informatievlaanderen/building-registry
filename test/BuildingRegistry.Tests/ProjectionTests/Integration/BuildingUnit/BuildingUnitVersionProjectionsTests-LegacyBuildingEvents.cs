// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Tests.ProjectionTests.Integration.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using BuildingRegistry.Building;
    using BuildingRegistry.Legacy.Events;
    using FluentAssertions;
    using Moq;
    using Projections.Integration.Converters;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using BuildingId = BuildingRegistry.Legacy.BuildingId;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using WithFixedBuildingId = Tests.Legacy.Autofixture.WithFixedBuildingId;

    public partial class BuildingUnitVersionProjectionsTests
    {
        [Fact]
        public async Task WhenBuildingWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasRemoved =  new BuildingWasRemoved(
                _fixture.Create<BuildingId>(),
                new [] { _fixture.Create<BuildingUnitId>() });
            ((ISetProvenance)buildingWasRemoved).SetProvenance(_fixture.Create<Provenance>());

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitWasAdded.AddressId))
                .ReturnsAsync(1);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasRemoved>(new Envelope(buildingWasRemoved, buildingWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.IsRemoved.Should().BeTrue();
                    buildingUnitVersion.Addresses.Should().BeEmpty();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRetired()
        {
            _fixture.Customize(new WithFixedBuildingId());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitToNotRealizeWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitToRetireWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasRetired =  new BuildingWasRetired(
                _fixture.Create<BuildingId>(),
                new [] { new BuildingUnitId(buildingUnitToRetireWasAdded.BuildingUnitId) },
                new [] { new BuildingUnitId(buildingUnitToNotRealizeWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasRetired).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitToNotRealizePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToNotRealizeWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToNotRealizePersistentLocalId);

            var buildingUnitToRetirePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToRetireWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToRetirePersistentLocalId);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToNotRealizeWasAdded.AddressId))
                .ReturnsAsync(1);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToRetireWasAdded.AddressId))
                .ReturnsAsync(2);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToNotRealizeWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToRetireWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasRetired>(new Envelope(buildingWasRetired, buildingWasRetiredMetadata)))
                .Then(async context =>
                {
                    var notRealizedBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToNotRealizePersistentLocalId);
                    notRealizedBuildingUnitVersion.Should().NotBeNull();

                    notRealizedBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Status);
                    notRealizedBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    notRealizedBuildingUnitVersion.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);

                    var retiredBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToRetirePersistentLocalId);
                    retiredBuildingUnitVersion.Should().NotBeNull();

                    retiredBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Retired.Status);
                    retiredBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Retired.Map());
                    retiredBuildingUnitVersion.Addresses.Should().BeEmpty();
                    retiredBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToRetired()
        {
            _fixture.Customize(new WithFixedBuildingId());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitToNotRealizeWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitToRetireWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasCorrectedToRetired =  new BuildingWasCorrectedToRetired(
                _fixture.Create<BuildingId>(),
                new [] { new BuildingUnitId(buildingUnitToRetireWasAdded.BuildingUnitId) },
                new [] { new BuildingUnitId(buildingUnitToNotRealizeWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasCorrectedToRetired).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitToNotRealizePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToNotRealizeWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToNotRealizePersistentLocalId);

            var buildingUnitToRetirePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToRetireWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToRetirePersistentLocalId);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToNotRealizeWasAdded.AddressId))
                .ReturnsAsync(1);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToRetireWasAdded.AddressId))
                .ReturnsAsync(2);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToRetiredMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToNotRealizeWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToRetireWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasCorrectedToRetired>(new Envelope(buildingWasCorrectedToRetired, buildingWasCorrectedToRetiredMetadata)))
                .Then(async context =>
                {
                    var notRealizedBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToNotRealizePersistentLocalId);
                    notRealizedBuildingUnitVersion.Should().NotBeNull();

                    notRealizedBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Status);
                    notRealizedBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    notRealizedBuildingUnitVersion.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);

                    var retiredBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToRetirePersistentLocalId);
                    retiredBuildingUnitVersion.Should().NotBeNull();

                    retiredBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Retired.Status);
                    retiredBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Retired.Map());
                    retiredBuildingUnitVersion.Addresses.Should().BeEmpty();
                    retiredBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitToNotRealizeWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitToRetireWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasNotRealized =  new BuildingWasNotRealized(
                _fixture.Create<BuildingId>(),
                new [] { new BuildingUnitId(buildingUnitToRetireWasAdded.BuildingUnitId) },
                new [] { new BuildingUnitId(buildingUnitToNotRealizeWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasNotRealized).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitToNotRealizePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToNotRealizeWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToNotRealizePersistentLocalId);

            var buildingUnitToRetirePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToRetireWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToRetirePersistentLocalId);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToNotRealizeWasAdded.AddressId))
                .ReturnsAsync(1);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToRetireWasAdded.AddressId))
                .ReturnsAsync(2);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToNotRealizeWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToRetireWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasNotRealized>(new Envelope(buildingWasNotRealized, buildingWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var notRealizedBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToNotRealizePersistentLocalId);
                    notRealizedBuildingUnitVersion.Should().NotBeNull();

                    notRealizedBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Status);
                    notRealizedBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    notRealizedBuildingUnitVersion.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);

                    var retiredBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToRetirePersistentLocalId);
                    retiredBuildingUnitVersion.Should().NotBeNull();

                    retiredBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Retired.Status);
                    retiredBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Retired.Map());
                    retiredBuildingUnitVersion.Addresses.Should().BeEmpty();
                    retiredBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedToNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingId());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitToNotRealizeWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitToRetireWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingWasCorrectedToNotRealized =  new BuildingWasCorrectedToNotRealized(
                _fixture.Create<BuildingId>(),
                new [] { new BuildingUnitId(buildingUnitToRetireWasAdded.BuildingUnitId) },
                new [] { new BuildingUnitId(buildingUnitToNotRealizeWasAdded.BuildingUnitId) });
            ((ISetProvenance)buildingWasCorrectedToNotRealized).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitToNotRealizePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToNotRealizeWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToNotRealizePersistentLocalId);

            var buildingUnitToRetirePersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x =>
                    x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), buildingUnitToRetireWasAdded.BuildingUnitId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitToRetirePersistentLocalId);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToNotRealizeWasAdded.AddressId))
                .ReturnsAsync(1);

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitToRetireWasAdded.AddressId))
                .ReturnsAsync(2);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasCorrectedToNotRealizedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToNotRealizeWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitToRetireWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingWasCorrectedToNotRealized>(new Envelope(buildingWasCorrectedToNotRealized, buildingWasCorrectedToNotRealizedMetadata)))
                .Then(async context =>
                {
                    var notRealizedBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToNotRealizePersistentLocalId);
                    notRealizedBuildingUnitVersion.Should().NotBeNull();

                    notRealizedBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Status);
                    notRealizedBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    notRealizedBuildingUnitVersion.Addresses.Should().BeEmpty();
                    notRealizedBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);

                    var retiredBuildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitToRetirePersistentLocalId);
                    retiredBuildingUnitVersion.Should().NotBeNull();

                    retiredBuildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Retired.Status);
                    retiredBuildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Retired.Map());
                    retiredBuildingUnitVersion.Addresses.Should().BeEmpty();
                    retiredBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasCorrectedToNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingGeometryWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitPositionWasAppointedByAdministrator = _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>();
            var buildingGeometryWasRemoved = _fixture.Create<BuildingGeometryWasRemoved>();

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitWasAdded.AddressId))
                .ReturnsAsync(1);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPositionWasAppointedByAdministratorMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingGeometryWasRemovedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingUnitPositionWasAppointedByAdministrator>(
                        new Envelope(buildingUnitPositionWasAppointedByAdministrator, buildingUnitPositionWasAppointedByAdministratorMetadata)),
                    new Envelope<BuildingGeometryWasRemoved>(new Envelope(buildingGeometryWasRemoved, buildingGeometryWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Geometry.Should().BeNull();
                    buildingUnitVersion.GeometryMethod.Should().BeNull();
                    buildingUnitVersion.OsloGeometryMethod.Should().BeNull();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingGeometryWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameComplete()
        {
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingBecameComplete = _fixture.Create<BuildingBecameComplete>();

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitWasAdded.AddressId))
                .ReturnsAsync(1);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingBecameCompleteMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingBecameComplete>(new Envelope(buildingBecameComplete, buildingBecameCompleteMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingBecameComplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameIncomplete()
        {
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingBecameIncomplete = _fixture.Create<BuildingBecameIncomplete>();

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitWasAdded.AddressId))
                .ReturnsAsync(1);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingBecameIncompleteMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingBecameIncomplete>(new Envelope(buildingBecameIncomplete, buildingBecameIncompleteMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingBecameIncomplete.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingPersistentLocalIdWasAssigned()
        {
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingPersistentLocalId(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int) _fixture.Create<BuildingPersistentLocalId>());

            var buildingUnitPersistentLocalId = (int) _fixture.Create<BuildingUnitPersistentLocalId>();

            _persistentLocalIdFinder
                .Setup(x => x.FindBuildingUnitPersistentLocalId(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(buildingUnitPersistentLocalId);

            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>();
            var buildingPersistentLocalIdWasAssigned = _fixture.Create<BuildingPersistentLocalIdWasAssigned>();

            _addresses
                .Setup(x => x.GetAddressPersistentLocalId(buildingUnitWasAdded.AddressId))
                .ReturnsAsync(1);

            var position = _fixture.Create<long>();

            var buildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position }
            };
            var buildingPersistentLocalIdWasAssignedMetadata = new Dictionary<string, object>
            {
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasAdded>(new Envelope(buildingUnitWasAdded, buildingUnitWasAddedMetadata)),
                    new Envelope<BuildingPersistentLocalIdWasAssigned>(new Envelope(buildingPersistentLocalIdWasAssigned, buildingPersistentLocalIdWasAssignedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingPersistentLocalIdWasAssigned.PersistentLocalId);
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingPersistentLocalIdWasAssigned.Provenance.Timestamp);
                });
        }
    }
}

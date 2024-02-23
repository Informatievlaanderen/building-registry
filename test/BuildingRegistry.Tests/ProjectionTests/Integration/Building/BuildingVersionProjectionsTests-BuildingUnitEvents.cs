// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
// ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery
namespace BuildingRegistry.Tests.ProjectionTests.Integration.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Projections.Integration.Converters;
    using Xunit;

    public sealed partial class BuildingVersionProjectionsTests
    {
        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var position = _fixture.Create<long>();
            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedV2Metadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingUnitWasPlannedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlannedV2.GeometryMethod).GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlannedV2.GeometryMethod).Map());
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasPlannedV2.Function).Function);
                    buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasPlannedV2.Function).Map());
                    buildingUnitVersion.Status.Should().Be("Planned");
                    buildingUnitVersion.OsloStatus.Should().Be("Gepland");
                    buildingUnitVersion.HasDeviation.Should().Be(buildingUnitWasPlannedV2.HasDeviation);
                    buildingUnitVersion.IsRemoved.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId}");
                    buildingUnitVersion.Type.Should().Be("EventName");

                    buildingVersion.VersionTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Realized");
                    buildingUnitVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingUnitVersion.Type.Should().Be("EventName");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedBecauseBuildingWasRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealized = _fixture.Create<BuildingUnitWasRealizedBecauseBuildingWasRealized>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealized.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>(new Envelope(buildingUnitWasRealized, buildingUnitWasRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Realized");
                    buildingUnitVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRealized.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasCorrectedFromRealizedToPlanned = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlanned>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedFromRealizedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>(
                        new Envelope(buildingUnitWasCorrectedFromRealizedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Planned");
                    buildingUnitVersion.OsloStatus.Should().Be("Gepland");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedFromRealizedToPlanned.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasCorrectedToPlanned = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(
                        new Envelope(buildingUnitWasCorrectedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Planned");
                    buildingUnitVersion.OsloStatus.Should().Be("Gepland");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealizedV2 = _fixture.Create<BuildingUnitWasNotRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedV2>(new Envelope(buildingUnitWasNotRealizedV2, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("NotRealized");
                    buildingUnitVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealizedV2.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedBecauseBuildingWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealized = _fixture.Create<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealized.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(
                        new Envelope(buildingUnitWasNotRealized, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("NotRealized");
                    buildingUnitVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromNotRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealizedV2 = _fixture.Create<BuildingUnitWasNotRealizedV2>();
            var buildingUnitWasCorrectedToPlanned = _fixture.Create<BuildingUnitWasCorrectedFromNotRealizedToPlanned>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedV2>(new Envelope(buildingUnitWasNotRealizedV2, buildingUnitWasNotRealizedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(buildingUnitWasCorrectedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Planned");
                    buildingUnitVersion.OsloStatus.Should().Be("Gepland");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasRetiredV2>(new Envelope(buildingUnitWasRetiredV2, buildingUnitWasRetiredMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Retired");
                    buildingUnitVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRetiredV2.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRetiredToRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasCorrectedToRealized = _fixture.Create<BuildingUnitWasCorrectedFromRetiredToRealized>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasCorrectedToRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToRealized.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasRetiredV2>(new Envelope(buildingUnitWasRetiredV2, buildingUnitWasRetiredMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>(
                        new Envelope(buildingUnitWasCorrectedToRealized, buildingUnitWasCorrectedToRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Realized");
                    buildingUnitVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToRealized.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitPositionWasCorrected = _fixture.Create<BuildingUnitPositionWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitPositionWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitPositionWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };
            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitPositionWasCorrected>(new Envelope(buildingUnitPositionWasCorrected, buildingUnitPositionWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingUnitPositionWasCorrected.BuildingPersistentLocalId);
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitPositionWasCorrected.GeometryMethod).GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitPositionWasCorrected.GeometryMethod).Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitPositionWasCorrected.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRemovedV2>(new Envelope(buildingUnitWasRemovedV2, buildingUnitWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.IsRemoved.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    buildingVersion.VersionTimestamp.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedBecauseBuildingWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemoved = _fixture.Create<BuildingUnitWasRemovedBecauseBuildingWasRemoved>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemoved.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>(new Envelope(buildingUnitWasRemoved, buildingUnitWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.IsRemoved.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRemoved.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    buildingVersion.VersionTimestamp.Should().Be(buildingUnitWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var buildingUnitRemovalWasCorrected = _fixture.Create<BuildingUnitRemovalWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitRemovalWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitRemovalWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRemovedV2>(new Envelope(buildingUnitWasRemovedV2, buildingUnitWasRemovedMetadata)),
                    new Envelope<BuildingUnitRemovalWasCorrected>(new Envelope(buildingUnitRemovalWasCorrected, buildingUnitRemovalWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Parse(buildingUnitRemovalWasCorrected.BuildingUnitStatus).Status);
                    buildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Parse(buildingUnitRemovalWasCorrected.BuildingUnitStatus).Map());
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnitRemovalWasCorrected.Function).Function);
                    buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnitRemovalWasCorrected.Function).Map());
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitRemovalWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitRemovalWasCorrected.GeometryMethod).GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitRemovalWasCorrected.GeometryMethod).Map());
                    buildingUnitVersion.HasDeviation.Should().Be(buildingUnitRemovalWasCorrected.HasDeviation);
                    buildingUnitVersion.IsRemoved.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    buildingVersion.VersionTimestamp.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRegularized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(true);
            var buildingUnitWasRegularized = _fixture.Create<BuildingUnitWasRegularized>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRegularizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRegularized.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRegularized>(new Envelope(buildingUnitWasRegularized, buildingUnitWasRegularizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRegularized.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRegularizationWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(false);
            var buildingUnitRegularizationWasCorrected = _fixture.Create<BuildingUnitRegularizationWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitRegularizationWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitRegularizationWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitRegularizationWasCorrected>(
                        new Envelope(buildingUnitRegularizationWasCorrected, buildingUnitRegularizationWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitRegularizationWasCorrected.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasDeregulated()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(false);
            var buildingUnitWasDeregulated = _fixture.Create<BuildingUnitWasDeregulated>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasDeregulatedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasDeregulated.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasDeregulated>(new Envelope(buildingUnitWasDeregulated, buildingUnitWasDeregulatedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasDeregulated.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitDeregulationWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(true);
            var buildingUnitDeregulationWasCorrected = _fixture.Create<BuildingUnitDeregulationWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitDeregulationWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitDeregulationWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };
            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitDeregulationWasCorrected>(
                        new Envelope(buildingUnitDeregulationWasCorrected, buildingUnitDeregulationWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitDeregulationWasCorrected.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var commonBuildingUnitWasAddedV2 = new CommonBuildingUnitWasAddedV2(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitStatus.Planned,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                _fixture.Create<ExtendedWkbGeometry>(),
                false);
            ((ISetProvenance)commonBuildingUnitWasAddedV2).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var commonBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, commonBuildingUnitWasAddedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(commonBuildingUnitWasAddedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(commonBuildingUnitWasAddedV2.GeometryMethod).GeometryMethod);
                    buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(commonBuildingUnitWasAddedV2.GeometryMethod).Map());
                    buildingUnitVersion.Function.Should().Be("Common");
                    buildingUnitVersion.OsloFunction.Should().Be("GemeenschappelijkDeel");
                    buildingUnitVersion.Status.Should().Be(BuildingUnitStatus.Parse(commonBuildingUnitWasAddedV2.BuildingUnitStatus).Status);
                    buildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Parse(commonBuildingUnitWasAddedV2.BuildingUnitStatus).Map());
                    buildingUnitVersion.HasDeviation.Should().Be(commonBuildingUnitWasAddedV2.HasDeviation);
                    buildingUnitVersion.IsRemoved.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId}");
                    buildingUnitVersion.Type.Should().Be("EventName");

                    buildingVersion.VersionTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasAttachedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttachedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttachedV2, buildingUnitAddressWasAttachedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasAttachedV2.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().HaveCount(1);
                    buildingUnitVersion.Addresses.Single().AddressPersistentLocalId.Should().Be(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetachedV2 = _fixture.Create<BuildingUnitAddressWasDetachedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetachedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedV2>(new Envelope(buildingUnitAddressWasDetachedV2, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetachedV2.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRetired()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>(
                        new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRejected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(
                        new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>(
                        new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>(
                        new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasReplacedBecauseAddressWasReaddressed()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasReplacedBecauseAddressWasReaddressed = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId + 1));
            ((ISetProvenance)buildingUnitAddressWasReplacedBecauseAddressWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
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
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            buildingUnitAddressWasReplacedBecauseAddressWasReaddressed,
                            buildingUnitAddressWasReplacedBecauseAddressWasReaddressedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");

                    var newAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.NewAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();

                    var oldAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.PreviousAddressPersistentLocalId);
                    oldAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredBecauseBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetired = _fixture.Create<BuildingUnitWasRetiredBecauseBuildingWasDemolished>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetired.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>(new Envelope(buildingUnitWasRetired, buildingUnitWasRetiredMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("Retired");
                    buildingUnitVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRetired.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedBecauseBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealized = _fixture.Create<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealized.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>(new Envelope(buildingUnitWasNotRealized, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingVersion = await context.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();
                    var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
                        x.BuildingUnitPersistentLocalId == buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be("NotRealized");
                    buildingUnitVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        // [Fact]
        // public async Task WhenBuildingUnitWasTransferred()
        // {
        //     var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
        //     var buildingUnitWasTransferred = new BuildingUnitWasTransferredBuilder(_fixture)
        //         .WithBuildingPersistentLocalId(buildingWasPlannedV2.BuildingPersistentLocalId)
        //         .Build();
        //
        //     var position = _fixture.Create<long>();
        //
        //     var buildingWasPlannedV2Metadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
        //         { Envelope.PositionMetadataKey, position }
        //     };
        //     var buildingUnitWasTransferredMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingUnitWasTransferred.GetHash() },
        //         { Envelope.PositionMetadataKey, ++position }
        //     };
        //
        //     await Sut
        //         .Given(
        //             new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
        //             new Envelope<BuildingUnitWasTransferred>(new Envelope(buildingUnitWasTransferred, buildingUnitWasTransferredMetadata)))
        //         .Then(async context =>
        //         {
        //             var buildingVersion = await context.BuildingVersions.FindAsync(position);
        //             buildingVersion.Should().NotBeNull();
        //             var buildingUnitVersion = buildingVersion!.BuildingUnits.SingleOrDefault(x =>
        //                 x.BuildingUnitPersistentLocalId == buildingUnitWasTransferred.BuildingUnitPersistentLocalId);
        //             buildingUnitVersion.Should().NotBeNull();
        //
        //             buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingUnitWasTransferred.BuildingPersistentLocalId);
        //             buildingUnitVersion.Status.Should().Be(BuildingUnitStatus.Parse(buildingUnitWasTransferred.Status).Status);
        //             buildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Parse(buildingUnitWasTransferred.Status).Map());
        //             buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasTransferred.Function).Function);
        //             buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasTransferred.Function).Map());
        //             buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasTransferred.GeometryMethod).GeometryMethod);
        //             buildingUnitVersion.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasTransferred.GeometryMethod).Map());
        //             buildingUnitVersion.Geometry.Should().BeEquivalentTo(
        //                 _wkbReader.Read(buildingUnitWasTransferred.ExtendedWkbGeometry.ToByteArray()));
        //             buildingUnitVersion.HasDeviation.Should().BeFalse();
        //             buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasTransferred.Provenance.Timestamp);
        //
        //             buildingUnitVersion.Addresses.Should().HaveCount(buildingUnitWasTransferred.AddressPersistentLocalIds.Count);
        //             foreach (var addressPersistentLocalId in buildingUnitWasTransferred.AddressPersistentLocalIds)
        //             {
        //                 buildingUnitVersion.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId)
        //                     .Should().NotBeNull();
        //             }
        //
        //             buildingVersion.VersionTimestamp.Should().Be(buildingUnitWasTransferred.Provenance.Timestamp);
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenBuildingUnitWasMoved()
        // {
        //     _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        //
        //     var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
        //     var buildingUnitWasPlanned = _fixture.Create<BuildingUnitWasPlannedV2>();
        //     var buildingUnitWasMoved = _fixture.Create<BuildingUnitWasMoved>();
        //
        //     var position = _fixture.Create<long>();
        //
        //     var buildingWasPlannedMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() },
        //         { Envelope.PositionMetadataKey, position }
        //     };
        //     var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlanned.GetHash() },
        //         { Envelope.PositionMetadataKey, ++position }
        //     };
        //     var buildingUnitWasMovedMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingUnitWasMoved.GetHash() },
        //         { Envelope.PositionMetadataKey, ++position }
        //     };
        //
        //     await Sut
        //         .Given(
        //             new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlanned, buildingWasPlannedMetadata)),
        //             new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlanned, buildingUnitWasPlannedMetadata)),
        //             new Envelope<BuildingUnitWasMoved>(new Envelope(buildingUnitWasMoved, buildingUnitWasMovedMetadata)))
        //         .Then(async ct =>
        //         {
        //             var buildingVersion = await ct.BuildingVersions.FindAsync(position);
        //             buildingVersion.Should().NotBeNull();
        //
        //             buildingVersion!.BuildingUnits.Should().BeEmpty();
        //             buildingVersion.VersionTimestamp.Should().Be(buildingUnitWasMoved.Provenance.Timestamp);
        //         });
        // }
    }
}

namespace BuildingRegistry.Tests.ProjectionTests.Integration
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;
    using Projections.Integration.Building.LatestItem;
    using Projections.Integration.Converters;
    using Projections.Integration.Infrastructure;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingLatestItemProjectionsTests : IntegrationProjectionTest<BuildingLatestItemProjections>
    {
        private const string BuildingNamespace = "https://data.vlaanderen.be/id/gebouw";
        private const string BuildingUnitNamespace = "https://data.vlaanderen.be/id/gebouweenheid";

        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader = WKBReaderFactory.Create();

        public BuildingLatestItemProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
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

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Parse(buildingWasMigrated.BuildingStatus).Map());
                    buildingLatestItem.OsloStatus.Should().Be(buildingWasMigrated.BuildingStatus);
                    buildingLatestItem.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasMigrated.ExtendedWkbGeometry.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod).Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be(buildingWasMigrated.GeometryMethod);
                    buildingLatestItem.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    buildingLatestItem.Namespace.Should().Be(BuildingNamespace);
                    buildingLatestItem.Puri.Should().Be($"{BuildingNamespace}/{buildingWasMigrated.BuildingPersistentLocalId}");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlannedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Planned.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Planned");
                    buildingLatestItem.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.Outlined.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("Outlined");
                    buildingLatestItem.IsRemoved.Should().BeFalse();
                    buildingLatestItem.Namespace.Should().Be(BuildingNamespace);
                    buildingLatestItem.Puri.Should().Be($"{BuildingNamespace}/{buildingWasPlannedV2.BuildingPersistentLocalId}");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenUnplannedBuildingWasRealizedAndMeasured()
        {
            var unplannedBuildingWasRealizedAndMeasured = _fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, unplannedBuildingWasRealizedAndMeasured.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<UnplannedBuildingWasRealizedAndMeasured>(new Envelope(unplannedBuildingWasRealizedAndMeasured, metadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem =
                        await ct.BuildingLatestItems.FindAsync(unplannedBuildingWasRealizedAndMeasured.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Realized.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Realized");
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.MeasuredByGrb.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("MeasuredByGrb");
                    buildingLatestItem.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(unplannedBuildingWasRealizedAndMeasured.ExtendedWkbGeometry.ToByteArray()));
                    buildingLatestItem.IsRemoved.Should().BeFalse();
                    buildingLatestItem.Namespace.Should().Be(BuildingNamespace);
                    buildingLatestItem.Puri.Should().Be($"{BuildingNamespace}/{unplannedBuildingWasRealizedAndMeasured.BuildingPersistentLocalId}");
                    buildingLatestItem.VersionTimestamp.Should().Be(unplannedBuildingWasRealizedAndMeasured.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var position = _fixture.Create<long>();
            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.VersionTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAdded()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();

            var position = _fixture.Create<long>();
            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var commonBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, commonBuildingUnitWasAddedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.VersionTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedV2()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var buildingUnitWasRemovedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasRemovedV2>(new Envelope(buildingUnitWasRemovedV2, buildingUnitWasRemovedV2Metadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingUnitWasRemovedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.VersionTimestamp.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var buildingUnitWasRemovedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            var buildingUnitRemovalWasCorrected = _fixture.Create<BuildingUnitRemovalWasCorrected>();
            var buildingUnitRemovalWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitRemovalWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 3 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedV2Metadata)),
                    new Envelope<BuildingUnitWasRemovedV2>(new Envelope(buildingUnitWasRemovedV2, buildingUnitWasRemovedV2Metadata)),
                    new Envelope<BuildingUnitRemovalWasCorrected>(new Envelope(buildingUnitRemovalWasCorrected, buildingUnitRemovalWasCorrectedMetadata))
                )
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingUnitRemovalWasCorrected.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.IsRemoved.Should().BeFalse();
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingOutlineWasChanged = _fixture.Create<BuildingOutlineWasChanged>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingOutlineWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingOutlineWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingOutlineWasChanged>(new Envelope(buildingOutlineWasChanged, buildingOutlineWasChangedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingOutlineWasChanged.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.Outlined.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("Outlined");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasChanged()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingOMeasurementWasChanged = _fixture.Create<BuildingMeasurementWasChanged>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingMeasurementWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingOMeasurementWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasChanged>(new Envelope(buildingOMeasurementWasChanged, buildingMeasurementWasChangedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingOMeasurementWasChanged.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingOMeasurementWasChanged.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.MeasuredByGrb.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("MeasuredByGrb");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingOMeasurementWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstructionV2()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstructionV2 = _fixture.Create<BuildingBecameUnderConstructionV2>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingBecameUnderConstructionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingBecameUnderConstructionV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingBecameUnderConstructionV2>(new Envelope(buildingBecameUnderConstructionV2,
                        buildingBecameUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingBecameUnderConstructionV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.UnderConstruction.Map());
                    buildingLatestItem.OsloStatus.Should().Be("UnderConstruction");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingBecameUnderConstructionV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromUnderConstructionToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasCorrectedFromUnderConstructionToPlanned = _fixture.Create<BuildingWasCorrectedFromUnderConstructionToPlanned>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromUnderConstructionToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>(
                        new Envelope(buildingWasCorrectedFromUnderConstructionToPlanned, buildingWasCorrectedToPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem =
                        await ct.BuildingLatestItems.FindAsync(buildingWasCorrectedFromUnderConstructionToPlanned.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Planned.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Planned");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasCorrectedFromUnderConstructionToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            var buildingWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasRealizedV2>(new Envelope(buildingWasRealizedV2, buildingWasRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Realized.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Realized");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromRealizedToUnderConstruction()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();
            var buildingWasCorrectedFromRealizedToUnderConstruction = _fixture.Create<BuildingWasCorrectedFromRealizedToUnderConstruction>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingWasCorrectedToUnderConstructionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromRealizedToUnderConstruction.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasRealizedV2>(
                        new Envelope(
                            buildingWasRealizedV2,
                            buildingWasRealizedMetadata)),
                    new Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>(
                        new Envelope(
                            buildingWasCorrectedFromRealizedToUnderConstruction,
                            buildingWasCorrectedToUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.UnderConstruction.Map());
                    buildingLatestItem.OsloStatus.Should().Be("UnderConstruction");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasCorrectedFromRealizedToUnderConstruction.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasNotRealizedV2>(new Envelope(buildingWasNotRealizedV2, buildingWasNotRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.NotRealized.Map());
                    buildingLatestItem.OsloStatus.Should().Be("NotRealized");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasNotRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromNotRealizedToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();
            var buildingWasCorrectedFromNotRealizedToPlanned = _fixture.Create<BuildingWasCorrectedFromNotRealizedToPlanned>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromNotRealizedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasNotRealizedV2>(
                        new Envelope(
                            buildingWasNotRealizedV2,
                            buildingWasNotRealizedMetadata)),
                    new Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(
                            buildingWasCorrectedFromNotRealizedToPlanned,
                            buildingWasCorrectedToPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Planned.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Planned");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasCorrectedFromNotRealizedToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRemovedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRemovedV2 = _fixture.Create<BuildingWasRemovedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasRemovedV2>(
                        new Envelope(
                            buildingWasRemovedV2,
                            buildingWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasRemovedV2.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.IsRemoved.Should().BeTrue();
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasRemovedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasured()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasured>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasMeasuredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMeasured.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasMeasured>(
                        new Envelope(
                            buildingWasMeasured,
                            buildingWasMeasuredMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasMeasured.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.MeasuredByGrb.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("MeasuredByGrb");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasCorrected()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasured>();
            var buildingMeasurementWasCorrected = _fixture.Create<BuildingMeasurementWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasMeasured>(
                        new Envelope(
                            buildingWasMeasured,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasCorrected.GetHash() },
                                { Envelope.PositionMetadataKey, position + 1 }
                            })),
                    new Envelope<BuildingMeasurementWasCorrected>(
                        new Envelope(
                            buildingMeasurementWasCorrected,
                            new Dictionary<string, object>
                            {
                                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasCorrected.GetHash() },
                                { Envelope.PositionMetadataKey, position + 2 }
                            })))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingMeasurementWasCorrected.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.MeasuredByGrb.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("MeasuredByGrb");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasDemolished = _fixture.Create<BuildingWasDemolished>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasDemolishedMetdata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasDemolished.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasDemolished>(
                        new Envelope(
                            buildingWasDemolished,
                            buildingWasDemolishedMetdata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasDemolished.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Retired.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Retired");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasDemolished.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMergerWasRealized()
        {
            var buildingMergerWasRealized = _fixture.Create<BuildingMergerWasRealized>();

            var position = _fixture.Create<long>();

            var buildingMergerWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMergerWasRealized.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(
                    new Envelope<BuildingMergerWasRealized>(
                        new Envelope(
                            buildingMergerWasRealized,
                            buildingMergerWasRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingMergerWasRealized.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.BuildingPersistentLocalId.Should().Be(buildingMergerWasRealized.BuildingPersistentLocalId);
                    buildingLatestItem.Status.Should().Be(BuildingStatus.Realized.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Realized");
                    buildingLatestItem.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingMergerWasRealized.ExtendedWkbGeometry.ToByteArray()));
                    buildingLatestItem.GeometryMethod.Should().Be(BuildingGeometryMethod.MeasuredByGrb.Map());
                    buildingLatestItem.OsloGeometryMethod.Should().Be("MeasuredByGrb");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingMergerWasRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMoved()
        {
            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasMoved = _fixture.Create<BuildingUnitWasMoved>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasMovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasMoved.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlanned, buildingWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlanned, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasMoved>(new Envelope(buildingUnitWasMoved, buildingUnitWasMovedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingUnitWasMoved.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.BuildingPersistentLocalId.Should().Be(buildingUnitWasMoved.BuildingPersistentLocalId);
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingUnitWasMoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMerged()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasMerged = _fixture.Create<BuildingWasMerged>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasMergedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMerged.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlanned,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasMerged>(
                        new Envelope(
                            buildingWasMerged,
                            buildingWasMergedMetadata)))
                .Then(async ct =>
                {
                    var buildingLatestItem = await ct.BuildingLatestItems.FindAsync(buildingWasMerged.BuildingPersistentLocalId);
                    buildingLatestItem.Should().NotBeNull();

                    buildingLatestItem!.Status.Should().Be(BuildingStatus.Retired.Map());
                    buildingLatestItem.OsloStatus.Should().Be("Retired");
                    buildingLatestItem.VersionTimestamp.Should().Be(buildingWasMerged.Provenance.Timestamp);
                });
        }

        protected override BuildingLatestItemProjections CreateProjection() =>
            new BuildingLatestItemProjections(new OptionsWrapper<IntegrationOptions>(new IntegrationOptions
            {
                BuildingNamespace = BuildingNamespace,
                BuildingUnitNamespace = BuildingUnitNamespace
            }));
    }
}

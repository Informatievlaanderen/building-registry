namespace BuildingRegistry.Tests.ProjectionTests.Wms
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
    using Infrastructure;
    using Projections.Wms.BuildingV3;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingV3Tests : BuildingWmsProjectionTest<BuildingV3Projections>
    {
        private readonly Fixture _fixture;

        public BuildingV3Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPolygon());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public async Task WhenNonRemovedBuildingWasMigrated()
        {
            _fixture.Register(() => false);
            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();

                    buildingDetailItem!.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(buildingWasMigrated.BuildingPersistentLocalId));
                    buildingDetailItem.Status.Should().Be(BuildingStatus.Parse(buildingWasMigrated.BuildingStatus));
                    buildingDetailItem.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);

                    buildingDetailItem.Geometry.Should().BeEquivalentTo(buildingWasMigrated.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItem.GeometryMethod.Should().Be(BuildingV3Projections.MapMethod(BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod)));
                });
        }

        [Fact]
        public async Task WhenRemovedBuildingWasMigrated()
        {
            _fixture.Register(() => true);
            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingDetailItem.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlannedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();

                    buildingDetailItem!.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(buildingWasPlannedV2.BuildingPersistentLocalId));
                    buildingDetailItem.Status.Should().Be(BuildingStatus.Planned);
                    buildingDetailItem.Version.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItem.Geometry.Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItem.GeometryMethod.Should().Be("Ingeschetst");
                });
        }

        [Fact]
        public async Task WhenUnplannedBuildingWasRealizedAndMeasured()
        {
            var @event = _fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, @event.GetHash() }
            };

            await Sut
                .Given(new Envelope<UnplannedBuildingWasRealizedAndMeasured>(new Envelope(@event, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();

                    buildingDetailItem!.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(@event.BuildingPersistentLocalId));
                    buildingDetailItem.Status.Should().Be(BuildingStatus.Realized);
                    buildingDetailItem.GeometryMethod.Should().Be("IngemetenGRB");
                    buildingDetailItem.Version.Should().Be(@event.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItem.Geometry.Should().BeEquivalentTo(polygon.AsBinary());
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingOutlineWasChanged = _fixture.Create<BuildingOutlineWasChanged>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingOutlineWasChanged>(
                        new Envelope(
                            buildingOutlineWasChanged,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingOutlineWasChanged.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingOutlineWasChanged.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItem.Geometry.Should().BeEquivalentTo(polygon.AsBinary());
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasChanged()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingMeasurementWasChanged = _fixture.Create<BuildingMeasurementWasChanged>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingMeasurementWasChanged>(
                        new Envelope(
                            buildingMeasurementWasChanged,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasChanged.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingMeasurementWasChanged.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                    buildingDetailItem.GeometryMethod.Should().Be(BuildingV3Projections.MeasuredByGrbMethod);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItem.Geometry.Should().BeEquivalentTo(polygon.AsBinary());
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstructionV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstructionV2 = _fixture.Create<BuildingBecameUnderConstructionV2>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingBecameUnderConstructionV2>(new Envelope(buildingBecameUnderConstructionV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingBecameUnderConstructionV2.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingBecameUnderConstructionV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(buildingBecameUnderConstructionV2.Provenance.Timestamp);
                    buildingDetailItem.Status.Should().Be(BuildingStatus.UnderConstruction);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromUnderConstructionToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasCorrectedFromUnderConstructionToPlanned = _fixture.Create<BuildingWasCorrectedFromUnderConstructionToPlanned>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>(new Envelope(buildingWasCorrectedFromUnderConstructionToPlanned, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromUnderConstructionToPlanned.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasCorrectedFromUnderConstructionToPlanned.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(buildingWasCorrectedFromUnderConstructionToPlanned.Provenance.Timestamp);
                    buildingDetailItem.Status.Should().Be(BuildingStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasRealizedV2>(
                        new Envelope(
                            buildingWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Status.Should().Be(BuildingStatus.Realized);
                    buildingDetailItem.Version.Should().Be(buildingWasRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromRealizedToUnderConstruction()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();
            var buildingWasCorrectedFromRealizedToUnderConstruction = _fixture.Create<BuildingWasCorrectedFromRealizedToUnderConstruction>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasRealizedV2>(
                        new Envelope(
                            buildingWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() } })),
                    new Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>(
                        new Envelope(
                            buildingWasCorrectedFromRealizedToUnderConstruction,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromRealizedToUnderConstruction.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Status.Should().Be(BuildingStatus.UnderConstruction);
                    buildingDetailItem.Version.Should().Be(buildingWasCorrectedFromRealizedToUnderConstruction.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingWasNotRealizedV2>(new Envelope(buildingWasNotRealizedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(buildingWasNotRealizedV2.Provenance.Timestamp);
                    buildingDetailItem.Status.Should().Be(BuildingStatus.NotRealized);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromNotRealizedToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();
            var buildingWasCorrectedFromNotRealizedToPlanned = _fixture.Create<BuildingWasCorrectedFromNotRealizedToPlanned>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasNotRealizedV2>(
                        new Envelope(
                            buildingWasNotRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() } })),
                    new Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(
                            buildingWasCorrectedFromNotRealizedToPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromNotRealizedToPlanned.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Status.Should().Be(BuildingStatus.Planned);
                    buildingDetailItem.Version.Should().Be(buildingWasCorrectedFromNotRealizedToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRemovedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRemovedV2 = _fixture.Create<BuildingWasRemovedV2>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasRemovedV2>(
                        new Envelope(
                            buildingWasRemovedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasRemovedV2.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(buildingWasRemovedV2.BuildingPersistentLocalId);
                    buildingDetailItem.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasured()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var @event = _fixture.Create<BuildingWasMeasured>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasMeasured>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(@event.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItem.Geometry.Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItem.GeometryMethod.Should().Be(BuildingV3Projections.MeasuredByGrbMethod);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasCorrected()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasured>();
            var @event = _fixture.Create<BuildingMeasurementWasCorrected>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasMeasured>(
                        new Envelope(
                            buildingWasMeasured,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })),
                    new Envelope<BuildingMeasurementWasCorrected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItem = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    buildingDetailItem.Should().NotBeNull();
                    buildingDetailItem!.Version.Should().Be(@event.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItem.Geometry.Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItem.GeometryMethod.Should().Be(BuildingV3Projections.MeasuredByGrbMethod);
                });
        }

        [Fact]
        public async Task WhenBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var buildingWasDemolished = _fixture.Create<BuildingWasDemolished>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasDemolished>(
                        new Envelope(
                            buildingWasDemolished,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasDemolished.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV3.FindAsync(buildingWasDemolished.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingStatus.Retired);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMovedIntoBuilding()
        {
            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var @event = _fixture.Create<BuildingUnitWasMovedIntoBuilding>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() } })),
                    new Envelope<BuildingUnitWasMovedIntoBuilding>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMovedOutOfBuilding()
        {
            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var @event = _fixture.Create<BuildingUnitWasMovedOutOfBuilding>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() } })),
                    new Envelope<BuildingUnitWasMovedOutOfBuilding>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemoved()
        {
            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var @event = _fixture.Create<BuildingUnitWasRemovedV2>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() } })),
                    new Envelope<BuildingUnitWasRemovedV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected()
        {
            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var @event = _fixture.Create<BuildingUnitRemovalWasCorrected>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() } })),
                    new Envelope<BuildingUnitRemovalWasCorrected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV3.FindAsync(@event.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        protected override BuildingV3Projections CreateProjection() => new BuildingV3Projections(WKBReaderFactory.Create());
    }
}

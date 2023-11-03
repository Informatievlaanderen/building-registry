namespace BuildingRegistry.Tests.ProjectionTests.Wfs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using Projections.Wfs.BuildingV2;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public class BuildingV2Tests : BuildingWfsProjectionTest<BuildingV2Projections>
    {
        private readonly Fixture _fixture;

        public BuildingV2Tests()
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
        public async Task WhenBuildingWasMigrated()
        {
            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2!.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(buildingWasMigrated.BuildingPersistentLocalId));
                    buildingDetailItemV2.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    buildingDetailItemV2.Status.Should()
                        .Be(BuildingV2Projections.MapStatus(BuildingStatus.Parse(buildingWasMigrated.BuildingStatus)));
                    buildingDetailItemV2.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingWasMigrated.ExtendedWkbGeometry.ToByteArray()) as Polygon;
                    buildingDetailItemV2.Geometry.Should().Be(new GrbPolygon(polygon!));
                    buildingDetailItemV2.GeometryMethod.Should()
                        .Be(BuildingV2Projections.MapGeometryMethod(BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod)));
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2!.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(buildingWasPlannedV2.BuildingPersistentLocalId));
                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Planned));
                    buildingDetailItemV2.Version.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItemV2.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingV2Projections.MapGeometryMethod(BuildingGeometryMethod.Outlined));
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(@event.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2!.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(@event.BuildingPersistentLocalId));
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Realized));
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingV2Projections.MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                    buildingDetailItemV2.Version.Should().Be(@event.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItemV2.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingOutlineWasChanged.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItemV2.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingMeasurementWasChanged.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingV2Projections.MeasuredMethod);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItemV2.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingBecameUnderConstructionV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingBecameUnderConstructionV2.Provenance.Timestamp);
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.UnderConstruction));
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
                    new Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>(new Envelope(buildingWasCorrectedFromUnderConstructionToPlanned,
                        new Dictionary<string, object>
                        {
                            { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromUnderConstructionToPlanned.GetHash() }
                        })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 =
                        await ct.BuildingsV2.FindAsync(buildingWasCorrectedFromUnderConstructionToPlanned.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingWasCorrectedFromUnderConstructionToPlanned.Provenance.Timestamp);
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Planned));
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Realized));
                    buildingDetailItemV2.Version.Should().Be(buildingWasRealizedV2.Provenance.Timestamp);
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
                            new Dictionary<string, object>
                                { { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromRealizedToUnderConstruction.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.UnderConstruction));
                    buildingDetailItemV2.Version.Should().Be(buildingWasCorrectedFromRealizedToUnderConstruction.Provenance.Timestamp);
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingWasNotRealizedV2.Provenance.Timestamp);
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.NotRealized));
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
                            new Dictionary<string, object>
                                { { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromNotRealizedToPlanned.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Planned));
                    buildingDetailItemV2.Version.Should().Be(buildingWasCorrectedFromNotRealizedToPlanned.Provenance.Timestamp);
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(buildingWasRemovedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.IsRemoved.Should().BeTrue();
                    buildingDetailItemV2.Version.Should().Be(buildingWasRemovedV2.Provenance.Timestamp);
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(@event.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(@event.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItemV2.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItemV2.GeometryMethod.Should().Be(GeometrieMethode.IngemetenGRB.ToString());
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
                    var buildingDetailItemV2 = await ct.BuildingsV2.FindAsync(@event.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(@event.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometryBuilding.ToByteArray());
                    buildingDetailItemV2.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItemV2.GeometryMethod.Should().Be(GeometrieMethode.IngemetenGRB.ToString());
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
                    var item = await ct.BuildingsV2.FindAsync(buildingWasDemolished.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be("Gehistoreerd");
                });
        }

        [Fact]
        public async Task WhenBuildingMergerWasRealized()
        {
            var @event = _fixture.Create<BuildingMergerWasRealized>();

            await Sut
                .Given(
                    new Envelope<BuildingMergerWasRealized>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV2.FindAsync(@event.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Realized));
                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(@event.ExtendedWkbGeometry.ToByteArray());
                    item.Geometry!.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
                    item.GeometryMethod.Should().Be(GeometrieMethode.IngemetenGRB.ToString());

                    item.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMerged()
        {
            var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
            var @event = _fixture.Create<BuildingWasMerged>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() } })),
                    new Envelope<BuildingWasMerged>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV2.FindAsync(@event.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Retired));
                    item.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasTransferred()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var oldBuildingPersistentLocalId = new BuildingPersistentLocalId(2);
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(3);

            var @event = new BuildingUnitWasTransferred(
                new BuildingPersistentLocalId(buildingWasPlannedV2.BuildingPersistentLocalId),
                BuildingUnit.Transfer(
                    _ => { },
                    new BuildingPersistentLocalId(buildingWasPlannedV2.BuildingPersistentLocalId),
                    buildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown,
                    BuildingUnitStatus.Realized,
                    new List<AddressPersistentLocalId>(),
                    new BuildingUnitPosition(new ExtendedWkbGeometry("".ToByteArray()), BuildingUnitPositionGeometryMethod.AppointedByAdministrator),
                    false), oldBuildingPersistentLocalId,
                new BuildingUnitPosition(new ExtendedWkbGeometry("".ToByteArray()), BuildingUnitPositionGeometryMethod.AppointedByAdministrator));
            @event.SetFixtureProvenance(_fixture);

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingUnitWasTransferred>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMoved()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var newBuildingPersistentLocalId = new BuildingPersistentLocalId(2);
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(3);

            var @event = new BuildingUnitWasMoved(
                new BuildingPersistentLocalId(buildingWasPlannedV2.BuildingPersistentLocalId),
                buildingUnitPersistentLocalId,
                newBuildingPersistentLocalId);
            @event.SetFixtureProvenance(_fixture);

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingUnitWasMoved>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Version.Should().Be(@event.Provenance.Timestamp);
                });
        }

        protected override BuildingV2Projections CreateProjection() => new BuildingV2Projections(WKBReaderFactory.Create());
    }
}

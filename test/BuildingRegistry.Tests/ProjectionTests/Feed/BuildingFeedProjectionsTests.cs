namespace BuildingRegistry.Tests.ProjectionTests.Feed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Building;
    using Building.Events;
    using CloudNative.CloudEvents;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Newtonsoft.Json;
    using Projections.Feed;
    using Projections.Feed.BuildingFeed;
    using Projections.Feed.Contract;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public sealed class BuildingFeedProjectionsTests
    {
        private const string NisCode = "11001";

        private readonly Fixture _fixture;
        private readonly FeedContext _feedContext;

        private ConnectedProjectionTest<FeedContext, BuildingFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }
        private Mock<IMunicipalityGeometryRepository> MunicipalityGeometryRepositoryMock { get; }

        public BuildingFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            MunicipalityGeometryRepositoryMock = new Mock<IMunicipalityGeometryRepository>();
            _feedContext = CreateContext();

            Sut = new ConnectedProjectionTest<FeedContext, BuildingFeedProjections>(
                () => _feedContext,
                () => new BuildingFeedProjections(ChangeFeedServiceMock.Object, MunicipalityGeometryRepositoryMock.Object));

            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPolygon());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());

            SetupChangeFeedServiceMock();
            SetupMunicipalityGeometryRepositoryMock();
        }

        [Fact]
        public async Task WhenBuildingWasMigrated_ThenFeedItemAndDocumentAreAdded()
        {
            _fixture.Register(() => BuildingStatus.Planned);
            _fixture.Register(() => BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasMigrated, position))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    document.RecordCreatedAt.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                    document.Document.VersionId.Should().Be(buildingWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset());

                    document.Document.PersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                    document.Document.Status.Should().Be(GebouwStatus.Gepland);
                    document.Document.GeometryMethod.Should().Be(GeometrieMethode.Ingeschetst);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(buildingWasMigrated.ExtendedWkbGeometry);

                    var feedItem = await FindFeedItemByBuildingPersistentLocalId(context, buildingWasMigrated.BuildingPersistentLocalId);
                    AssertFeedItem(feedItem, position, buildingWasMigrated);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.CreateV1,
                            buildingWasMigrated.BuildingPersistentLocalId.ToString(),
                            buildingWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.StatusName)
                                && attrs.Any(a => a.Name == BuildingAttributeNames.GeometryMethod)
                                && attrs.Any(a => a.Name == BuildingAttributeNames.Geometry)),
                            BuildingWasMigrated.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                    ChangeFeedServiceMock.Verify(x => x.CheckToUpdateCacheAsync(1, context, It.IsAny<Func<int, Task<int>>>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlannedV2_ThenFeedItemAndDocumentAreAdded()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.RecordCreatedAt.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                    document.Document.VersionId.Should().Be(buildingWasPlannedV2.Provenance.Timestamp.ToBelgianDateTimeOffset());

                    document.Document.PersistentLocalId.Should().Be(buildingWasPlannedV2.BuildingPersistentLocalId);
                    document.Document.Status.Should().Be(GebouwStatus.Gepland);
                    document.Document.GeometryMethod.Should().Be(GeometrieMethode.Ingeschetst);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(buildingWasPlannedV2.ExtendedWkbGeometry);

                    var feedItem = await FindFeedItemByBuildingPersistentLocalId(context, buildingWasPlannedV2.BuildingPersistentLocalId);
                    AssertFeedItem(feedItem, position, buildingWasPlannedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingWasPlannedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.CreateV1,
                            buildingWasPlannedV2.BuildingPersistentLocalId.ToString(),
                            buildingWasPlannedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == nameof(GebouwStatus.Gepland))
                                && attrs.Any(a => a.Name == BuildingAttributeNames.GeometryMethod
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == nameof(GeometrieMethode.Ingeschetst))
                                && attrs.Any(a => a.Name == BuildingAttributeNames.Geometry
                                               && a.OldValue == null
                                               && a.NewValue != null)),
                            BuildingWasPlannedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Once);
                });
        }

        [Fact]
        public async Task WhenUnplannedBuildingWasRealizedAndMeasured_ThenFeedItemAndDocumentAreAdded()
        {
            var @event = _fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(@event, position))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(@event.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.Document.Status.Should().Be(GebouwStatus.Gerealiseerd);
                    document.Document.GeometryMethod.Should().Be(GeometrieMethode.IngemetenGRB);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(@event.ExtendedWkbGeometry);

                    var feedItem = await FindFeedItemByBuildingPersistentLocalId(context, @event.BuildingPersistentLocalId);
                    AssertFeedItem(feedItem, position, @event);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            @event.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.CreateV1,
                            @event.BuildingPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.StatusName)
                                && attrs.Any(a => a.Name == BuildingAttributeNames.GeometryMethod)
                                && attrs.Any(a => a.Name == BuildingAttributeNames.Geometry)),
                            UnplannedBuildingWasRealizedAndMeasured.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstructionV2_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstructionV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingBecameUnderConstruction, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingBecameUnderConstruction.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.InAanbouw);
                    document.LastChangedOn.Should().Be(buildingBecameUnderConstruction.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingBecameUnderConstruction.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.UpdateV1,
                            buildingBecameUnderConstruction.BuildingPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(GebouwStatus.Gepland)
                                               && a.NewValue!.ToString() == nameof(GebouwStatus.InAanbouw))),
                            BuildingBecameUnderConstructionV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.Exactly(2));
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealizedV2_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstructionV2>();
            var buildingWasRealized = _fixture.Create<BuildingWasRealizedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingBecameUnderConstruction, position + 1),
                    CreateEnvelope(buildingWasRealized, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasRealized.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.Gerealiseerd);
                    document.LastChangedOn.Should().Be(buildingWasRealized.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingWasRealized.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.UpdateV1,
                            buildingWasRealized.BuildingPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(GebouwStatus.InAanbouw)
                                               && a.NewValue!.ToString() == nameof(GebouwStatus.Gerealiseerd))),
                            BuildingWasRealizedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromUnderConstructionToPlanned_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstructionV2>();
            var buildingWasCorrected = _fixture.Create<BuildingWasCorrectedFromUnderConstructionToPlanned>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingBecameUnderConstruction, position + 1),
                    CreateEnvelope(buildingWasCorrected, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasCorrected.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.Gepland);
                    document.LastChangedOn.Should().Be(buildingWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromRealizedToUnderConstruction_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstructionV2>();
            var buildingWasRealized = _fixture.Create<BuildingWasRealizedV2>();
            var buildingWasCorrected = _fixture.Create<BuildingWasCorrectedFromRealizedToUnderConstruction>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingBecameUnderConstruction, position + 1),
                    CreateEnvelope(buildingWasRealized, position + 2),
                    CreateEnvelope(buildingWasCorrected, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasCorrected.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.InAanbouw);
                    document.LastChangedOn.Should().Be(buildingWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealizedV2_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealized = _fixture.Create<BuildingWasNotRealizedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingWasNotRealized, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasNotRealized.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.NietGerealiseerd);
                    document.LastChangedOn.Should().Be(buildingWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromNotRealizedToPlanned_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealized = _fixture.Create<BuildingWasNotRealizedV2>();
            var buildingWasCorrected = _fixture.Create<BuildingWasCorrectedFromNotRealizedToPlanned>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingWasNotRealized, position + 1),
                    CreateEnvelope(buildingWasCorrected, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasCorrected.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.Gepland);
                    document.LastChangedOn.Should().Be(buildingWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasDemolished_ThenStatusIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstruction = _fixture.Create<BuildingBecameUnderConstructionV2>();
            var buildingWasRealized = _fixture.Create<BuildingWasRealizedV2>();
            var buildingWasDemolished = _fixture.Create<BuildingWasDemolished>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingBecameUnderConstruction, position + 1),
                    CreateEnvelope(buildingWasRealized, position + 2),
                    CreateEnvelope(buildingWasDemolished, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasDemolished.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouwStatus.Gehistoreerd);
                    document.LastChangedOn.Should().Be(buildingWasDemolished.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRemovedV2_ThenDocumentIsMarkedRemoved()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRemoved = _fixture.Create<BuildingWasRemovedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingWasRemoved, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasRemoved.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeTrue();
                    document.LastChangedOn.Should().Be(buildingWasRemoved.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingWasRemoved.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.DeleteV1,
                            buildingWasRemoved.BuildingPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs => !attrs.Any()),
                            BuildingWasRemovedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged_ThenGeometryIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingOutlineWasChanged = _fixture.Create<BuildingOutlineWasChanged>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingOutlineWasChanged, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingOutlineWasChanged.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.ExtendedWkbGeometry.Should().Be(buildingOutlineWasChanged.ExtendedWkbGeometryBuilding);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.LastChangedOn.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingOutlineWasChanged.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.UpdateV1,
                            buildingOutlineWasChanged.BuildingPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.Geometry
                                               && a.OldValue != null
                                               && a.NewValue != null)),
                            BuildingOutlineWasChanged.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasChanged_ThenGeometryIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingMeasurementWasChanged = _fixture.Create<BuildingMeasurementWasChanged>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingMeasurementWasChanged, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingMeasurementWasChanged.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.ExtendedWkbGeometry.Should().Be(buildingMeasurementWasChanged.ExtendedWkbGeometryBuilding);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.LastChangedOn.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasured_ThenGeometryAndMethodAreUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasured>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingWasMeasured, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingWasMeasured.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.ExtendedWkbGeometry.Should().Be(buildingWasMeasured.ExtendedWkbGeometryBuilding);
                    document.Document.GeometryMethod.Should().Be(GeometrieMethode.IngemetenGRB);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.LastChangedOn.Should().Be(buildingWasMeasured.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingWasMeasured.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingEventTypes.UpdateV1,
                            buildingWasMeasured.BuildingPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingAttributeNames.Geometry)
                                && attrs.Any(a => a.Name == BuildingAttributeNames.GeometryMethod
                                               && a.OldValue!.ToString() == nameof(GeometrieMethode.Ingeschetst)
                                               && a.NewValue!.ToString() == nameof(GeometrieMethode.IngemetenGRB))),
                            BuildingWasMeasured.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasCorrected_ThenGeometryIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingMeasurementWasCorrected = _fixture.Create<BuildingMeasurementWasCorrected>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingMeasurementWasCorrected, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingMeasurementWasCorrected.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.ExtendedWkbGeometry.Should().Be(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuilding);
                    document.Document.GeometryAsGml.Should().NotBeNullOrEmpty();
                    document.LastChangedOn.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                });
        }

        #region BuildingUnit events that update building version

        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2_ThenBuildingVersionIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2_ThenBuildingVersionIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(commonBuildingUnitWasAddedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedV2_ThenBuildingVersionIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitWasRemovedV2, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingUnitWasRemovedV2.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected_ThenBuildingVersionIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var buildingUnitRemovalWasCorrected = _fixture.Create<BuildingUnitRemovalWasCorrected>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitWasRemovedV2, position + 2),
                    CreateEnvelope(buildingUnitRemovalWasCorrected, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingUnitRemovalWasCorrected.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMovedIntoBuilding_ThenBuildingVersionIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasMovedIntoBuilding = _fixture.Create<BuildingUnitWasMovedIntoBuilding>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasMovedIntoBuilding, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingUnitWasMovedIntoBuilding.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(buildingUnitWasMovedIntoBuilding.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMovedOutOfBuilding_ThenBuildingVersionIsUpdated()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasMovedOutOfBuilding = _fixture.Create<BuildingUnitWasMovedOutOfBuilding>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasMovedOutOfBuilding, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingDocuments.FindAsync(buildingUnitWasMovedOutOfBuilding.BuildingPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.LastChangedOn.Should().Be(buildingUnitWasMovedOutOfBuilding.Provenance.Timestamp);
                });
        }

        #endregion

        #region Helpers

        private static void AssertFeedItem(
            BuildingFeedItem? feedItem,
            long position,
            IBuildingEvent @event)
        {
            feedItem.Should().NotBeNull();
            feedItem!.CloudEventAsString.Should().NotBeNullOrEmpty();
            feedItem.Page.Should().Be(1);
            feedItem.Position.Should().Be(position);
            feedItem.Application.Should().Be(@event.Provenance.Application);
            feedItem.Modification.Should().Be(@event.Provenance.Modification);
            feedItem.Operator.Should().Be(@event.Provenance.Operator);
            feedItem.Organisation.Should().Be(@event.Provenance.Organisation);
            feedItem.Reason.Should().Be(@event.Provenance.Reason);
        }

        private static async Task<BuildingFeedItem?> FindFeedItemByBuildingPersistentLocalId(FeedContext context, int buildingPersistentLocalId)
        {
            return await context.BuildingFeed
                .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                .FirstOrDefaultAsync();
        }

        private Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<T> CreateEnvelope<T>(T @event, long position) where T : IMessage
        {
            var metadata = new Dictionary<string, object>
            {
                { "Position", position },
                { "EventName", @event.GetType().Name },
                { "CommandId", Guid.NewGuid().ToString() }
            };
            return new Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<T>(
                new Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope(@event, metadata));
        }

        private void SetupChangeFeedServiceMock()
        {
            ChangeFeedServiceMock.Setup(x => x.CreateCloudEventWithData(
                    It.IsAny<long>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(new CloudEvent());

            ChangeFeedServiceMock.Setup(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>())).Returns("serialized cloud event");

            ChangeFeedServiceMock.Setup(x => x.CheckToUpdateCacheAsync(
                It.IsAny<int>(),
                It.IsAny<FeedContext>(),
                It.IsAny<Func<int, Task<int>>>()));
        }

        private void SetupMunicipalityGeometryRepositoryMock(List<string>? nisCodes = null)
        {
            MunicipalityGeometryRepositoryMock
                .Setup(x => x.GetOverlappingNisCodes(It.IsAny<string>(), It.IsAny<NodaTime.Instant>()))
                .Returns(nisCodes ?? new List<string> { NisCode });
        }

        private FeedContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<FeedContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new FeedContext(options, new JsonSerializerSettings());
        }

        #endregion
    }
}

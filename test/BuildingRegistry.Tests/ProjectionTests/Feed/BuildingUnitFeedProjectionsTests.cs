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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using CloudNative.CloudEvents;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using Projections.Feed;
    using Projections.Feed.BuildingFeed;
    using Projections.Feed.BuildingUnitFeed;
    using Projections.Feed.Contract;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public sealed class BuildingUnitFeedProjectionsTests
    {
        private const string NisCode = "11001";
        private static readonly string AddressNamespace = OsloNamespaces.Adres;

        private readonly Fixture _fixture;
        private readonly FeedContext _feedContext;

        private ConnectedProjectionTest<FeedContext, BuildingUnitFeedProjections> Sut { get; }
        private Mock<IChangeFeedService> ChangeFeedServiceMock { get; }
        private Mock<IMunicipalityGeometryRepository> MunicipalityGeometryRepositoryMock { get; }

        public BuildingUnitFeedProjectionsTests()
        {
            ChangeFeedServiceMock = new Mock<IChangeFeedService>();
            MunicipalityGeometryRepositoryMock = new Mock<IMunicipalityGeometryRepository>();
            _feedContext = CreateContext();

            Sut = new ConnectedProjectionTest<FeedContext, BuildingUnitFeedProjections>(
                () => _feedContext,
                () => new BuildingUnitFeedProjections(ChangeFeedServiceMock.Object, MunicipalityGeometryRepositoryMock.Object));

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
        public async Task WhenBuildingWasMigrated_ThenBuildingGeometryIsStoredAndBuildingUnitDocumentsCreated()
        {
            _fixture.Register(() => BuildingStatus.Planned);
            _fixture.Register(() => BuildingGeometryMethod.Outlined);
            _fixture.Register(() => false);
            _fixture.Customize(new WithValidExtendedWkbPoint());
            _fixture.RepeatCount = 1;

            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasMigrated, position))
                .Then(async context =>
                {
                    var buildingGeometry = await context.BuildingGeometryForBuildingUnit
                        .FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingGeometry.Should().NotBeNull();
                    buildingGeometry!.ExtendedWkbGeometry.Should().Be(buildingWasMigrated.ExtendedWkbGeometry);

                    foreach (var buildingUnit in buildingWasMigrated.BuildingUnits)
                    {
                        var document = await context.BuildingUnitDocuments.FindAsync(buildingUnit.BuildingUnitPersistentLocalId);
                        document.Should().NotBeNull();
                        document!.IsRemoved.Should().Be(buildingUnit.IsRemoved);
                        document.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        document.RecordCreatedAt.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        document.LastChangedOn.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        document.Document.PersistentLocalId.Should().Be(buildingUnit.BuildingUnitPersistentLocalId);
                        document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                        document.Document.ExtendedWkbGeometry.Should().Be(buildingUnit.ExtendedWkbGeometry);
                        document.Document.AddressPersistentLocalIds.Should().BeEquivalentTo(buildingUnit.AddressPersistentLocalIds);

                        var feedItem = await FindFeedItemByBuildingUnitPersistentLocalId(context, buildingUnit.BuildingUnitPersistentLocalId);
                        feedItem.Should().NotBeNull();
                        feedItem!.CloudEventAsString.Should().NotBeNullOrEmpty();

                        var expectedAddressPuris = buildingUnit.AddressPersistentLocalIds
                            .Select(id => $"{AddressNamespace}/{id}")
                            .Distinct()
                            .ToList();

                        ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                                It.IsAny<long>(),
                                buildingWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                                BuildingUnitEventTypes.CreateV1,
                                buildingUnit.BuildingUnitPersistentLocalId.ToString(),
                                buildingWasMigrated.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                                It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                                It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                    attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName && a.OldValue == null)
                                    && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Function && a.OldValue == null)
                                    && attrs.Any(a => a.Name == BuildingUnitAttributeNames.GeometryMethod && a.OldValue == null)
                                    && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Position && a.OldValue == null && a.NewValue != null)
                                    && attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                                   && a.OldValue == null
                                                   && a.NewValue != null
                                                   && ((List<string>)a.NewValue).SequenceEqual(expectedAddressPuris))),
                                BuildingWasMigrated.EventName,
                                It.IsAny<string>()),
                            Times.Once);
                    }

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.AtLeastOnce);
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlannedV2_ThenBuildingGeometryIsStored()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position))
                .Then(async context =>
                {
                    var buildingGeometry = await context.BuildingGeometryForBuildingUnit
                        .FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingGeometry.Should().NotBeNull();
                    buildingGeometry!.ExtendedWkbGeometry.Should().Be(buildingWasPlannedV2.ExtendedWkbGeometry);

                    var feedItems = await context.BuildingUnitFeed.ToListAsync();
                    feedItems.Should().BeEmpty();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            It.IsAny<DateTimeOffset>(),
                            It.IsAny<List<string>>(),
                            It.IsAny<List<BaseRegistriesCloudEventAttribute>>(),
                            It.IsAny<string>(),
                            It.IsAny<string>()),
                        Times.Never);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2_ThenDocumentAndFeedItemCreated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    document.RecordCreatedAt.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    document.Document.VersionId.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp.ToBelgianDateTimeOffset());

                    document.Document.PersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    document.Document.Status.Should().Be(GebouweenheidStatus.Gepland);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(buildingUnitWasPlannedV2.ExtendedWkbGeometry);
                    document.Document.AddressPersistentLocalIds.Should().BeEmpty();

                    var feedItem = await FindFeedItemByBuildingUnitPersistentLocalId(context, buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, buildingUnitWasPlannedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitWasPlannedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.CreateV1,
                            buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId.ToString(),
                            buildingUnitWasPlannedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == nameof(GebouweenheidStatus.Gepland))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Function
                                                  && a.OldValue == null)
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.GeometryMethod
                                                  && a.OldValue == null)
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Position
                                                  && a.OldValue == null
                                                  && a.NewValue != null && AssertPointList((List<BuildingUnitPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                                  && a.OldValue == null
                                                  && a.NewValue != null
                                                  && !((List<string>)a.NewValue).Any())),
                            BuildingUnitWasPlannedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.AtLeastOnce);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedV2_ThenStatusIsUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealized = _fixture.Create<BuildingUnitWasRealizedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitWasRealized, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitWasRealized.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouweenheidStatus.Gerealiseerd);
                    document.LastChangedOn.Should().Be(buildingUnitWasRealized.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitWasRealized.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitWasRealized.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(GebouweenheidStatus.Gepland)
                                               && a.NewValue!.ToString() == nameof(GebouweenheidStatus.Gerealiseerd))),
                            BuildingUnitWasRealizedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.AtLeastOnce);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredV2_ThenStatusIsUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            _fixture.Register(() => BuildingUnitStatus.Realized);
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealized = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasRetired = _fixture.Create<BuildingUnitWasRetiredV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitWasRealized, position + 2),
                    CreateEnvelope(buildingUnitWasRetired, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitWasRetired.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouweenheidStatus.Gehistoreerd);
                    document.LastChangedOn.Should().Be(buildingUnitWasRetired.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitWasRetired.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitWasRetired.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(GebouweenheidStatus.Gerealiseerd)
                                               && a.NewValue!.ToString() == nameof(GebouweenheidStatus.Gehistoreerd))),
                            BuildingUnitWasRetiredV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedV2_ThenStatusIsUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealized = _fixture.Create<BuildingUnitWasNotRealizedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitWasNotRealized, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitWasNotRealized.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.Status.Should().Be(GebouweenheidStatus.NietGerealiseerd);
                    document.LastChangedOn.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitWasNotRealized.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitWasNotRealized.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName
                                               && a.OldValue!.ToString() == nameof(GebouweenheidStatus.Gepland)
                                               && a.NewValue!.ToString() == nameof(GebouweenheidStatus.NietGerealiseerd))),
                            BuildingUnitWasNotRealizedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasCorrected_ThenPositionIsUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitPositionWasCorrected = _fixture.Create<BuildingUnitPositionWasCorrected>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitPositionWasCorrected, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitPositionWasCorrected.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.ExtendedWkbGeometry.Should().Be(buildingUnitPositionWasCorrected.ExtendedWkbGeometry);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.LastChangedOn.Should().Be(buildingUnitPositionWasCorrected.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitPositionWasCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitPositionWasCorrected.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.Position
                                               && a.OldValue != null && ((List<BuildingUnitPositionCloudEventValue>)a.OldValue).Count == 2
                                               && a.NewValue != null && AssertPointList((List<BuildingUnitPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))),
                            BuildingUnitPositionWasCorrected.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedV2_ThenDeleteEventCreated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
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
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitWasRemovedV2.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeTrue();
                    document.LastChangedOn.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitWasRemovedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.DeleteV1,
                            buildingUnitWasRemovedV2.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs => !attrs.Any()),
                            BuildingUnitWasRemovedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected_ThenCreateEventCreated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            _fixture.Register(() => BuildingUnitStatus.Planned);
            _fixture.Register(() => BuildingUnitFunction.Unknown);
            _fixture.Register(() => BuildingUnitPositionGeometryMethod.AppointedByAdministrator);

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
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitRemovalWasCorrected.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.Document.Status.Should().Be(GebouweenheidStatus.Gepland);
                    document.Document.Function.Should().Be(GebouweenheidFunctie.NietGekend);
                    document.Document.GeometryMethod.Should().Be(PositieGeometrieMethode.AangeduidDoorBeheerder);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(buildingUnitRemovalWasCorrected.ExtendedWkbGeometry);
                    document.Document.AddressPersistentLocalIds.Should().BeEmpty();
                    document.LastChangedOn.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitRemovalWasCorrected.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.CreateV1,
                            buildingUnitRemovalWasCorrected.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName
                                               && a.OldValue == null
                                               && a.NewValue!.ToString() == nameof(GebouweenheidStatus.Gepland))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Function
                                                  && a.OldValue == null
                                                  && a.NewValue!.ToString() == nameof(GebouweenheidFunctie.NietGekend))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.GeometryMethod
                                                  && a.OldValue == null
                                                  && a.NewValue!.ToString() == nameof(PositieGeometrieMethode.AangeduidDoorBeheerder))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Position
                                                  && a.OldValue == null
                                                  && a.NewValue != null && AssertPointList((List<BuildingUnitPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                                  && a.OldValue == null
                                                  && a.NewValue != null
                                                  && !((List<string>)a.NewValue).Any())),
                            BuildingUnitRemovalWasCorrected.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2_ThenDocumentAndFeedItemCreated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            _fixture.Register(() => BuildingUnitStatus.Planned);
            _fixture.Register(() => BuildingUnitPositionGeometryMethod.DerivedFromObject);

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(commonBuildingUnitWasAddedV2, position + 1))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.IsRemoved.Should().BeFalse();
                    document.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    document.RecordCreatedAt.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    document.LastChangedOn.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    document.Document.VersionId.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp.ToBelgianDateTimeOffset());

                    document.Document.PersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    document.Document.Function.Should().Be(GebouweenheidFunctie.GemeenschappelijkDeel);
                    document.Document.PositionAsGml.Should().NotBeNullOrEmpty();
                    document.Document.ExtendedWkbGeometry.Should().Be(commonBuildingUnitWasAddedV2.ExtendedWkbGeometry);
                    document.Document.AddressPersistentLocalIds.Should().BeEmpty();

                    var feedItem = await FindFeedItemByBuildingUnitPersistentLocalId(context, commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    AssertFeedItem(feedItem, position + 1, commonBuildingUnitWasAddedV2);

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            commonBuildingUnitWasAddedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.CreateV1,
                            commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId.ToString(),
                            commonBuildingUnitWasAddedV2.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.StatusName
                                               && a.OldValue == null)
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Function
                                                  && a.OldValue == null
                                                  && a.NewValue!.ToString() == nameof(GebouweenheidFunctie.GemeenschappelijkDeel))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.GeometryMethod
                                                  && a.OldValue == null)
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.Position
                                                  && a.OldValue == null
                                                  && a.NewValue != null && AssertPointList((List<BuildingUnitPositionCloudEventValue>)a.NewValue, document.Document.PositionAsGml))
                                && attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                                  && a.OldValue == null
                                                  && a.NewValue != null
                                                  && !((List<string>)a.NewValue).Any())),
                            CommonBuildingUnitWasAddedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);

                    ChangeFeedServiceMock.Verify(x => x.SerializeCloudEvent(It.IsAny<CloudEvent>()), Times.AtLeastOnce);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasAttachedV2_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasAttached.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(buildingUnitAddressWasAttached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasAttached.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>();

                    var expectedAddressPuris = oldAddressPuris
                        .Concat([$"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasAttached.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasAttached.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasAttachedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedV2_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = new BuildingUnitAddressWasDetachedV2(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId));
            ((ISetProvenance)buildingUnitAddressWasDetached).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitAddressWasDetached, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasDetached.AddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasDetached.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasDetachedV2.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRejected_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = new BuildingUnitAddressWasDetachedBecauseAddressWasRejected(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId));
            ((ISetProvenance)buildingUnitAddressWasDetached).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitAddressWasDetached, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasDetached.AddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasDetached.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasDetachedBecauseAddressWasRejected.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRemoved_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = new BuildingUnitAddressWasDetachedBecauseAddressWasRemoved(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId));
            ((ISetProvenance)buildingUnitAddressWasDetached).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitAddressWasDetached, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasDetached.AddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasDetached.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasDetachedBecauseAddressWasRemoved.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRetired_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = new BuildingUnitAddressWasDetachedBecauseAddressWasRetired(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId));
            ((ISetProvenance)buildingUnitAddressWasDetached).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitAddressWasDetached, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasDetached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasDetached.AddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasDetached.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasDetachedBecauseAddressWasRetired.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasReplacedBecauseAddressWasReaddressed_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            var newAddressId = _fixture.Create<int>();
            var buildingUnitAddressWasReplaced = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId),
                new AddressPersistentLocalId(newAddressId));
            ((ISetProvenance)buildingUnitAddressWasReplaced).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitAddressWasReplaced, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasReplaced.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(newAddressId);
                    document.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasAttached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasReplaced.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasReplaced.PreviousAddressPersistentLocalId}"])
                        .Concat([$"{AddressNamespace}/{buildingUnitAddressWasReplaced.NewAddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasReplaced.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasReplaced.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            var newAddressId = _fixture.Create<int>();
            var buildingUnitAddressWasReplaced = new BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(newAddressId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId));
            ((ISetProvenance)buildingUnitAddressWasReplaced).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitAddressWasReplaced, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitAddressWasReplaced.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(newAddressId);
                    document.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasAttached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitAddressWasReplaced.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasReplaced.PreviousAddressPersistentLocalId}"])
                        .Concat([$"{AddressNamespace}/{buildingUnitAddressWasReplaced.NewAddressPersistentLocalId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitAddressWasReplaced.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitAddressWasReplaced.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        [Fact]
        public async Task WhenBuildingBuildingUnitsAddressesWereReaddressed_ThenAddressesAreUpdated()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            var newAddressId = _fixture.Create<int>();
            var buildingUnitsAddressesWereReaddressed = new BuildingBuildingUnitsAddressesWereReaddressed(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new[]
                {
                    new BuildingUnitAddressesWereReaddressed(
                        new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                        new[] { new AddressPersistentLocalId(newAddressId) },
                        new[] { new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId) })
                },
                new[]
                {
                    new AddressRegistryReaddress(
                        new ReaddressData(
                            new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId),
                            new AddressPersistentLocalId(newAddressId)))
                });
            ((ISetProvenance)buildingUnitsAddressesWereReaddressed).SetProvenance(_fixture.Create<Provenance>());
            var position = 1L;

            await Sut
                .Given(CreateEnvelope(buildingWasPlannedV2, position),
                    CreateEnvelope(buildingUnitWasPlannedV2, position + 1),
                    CreateEnvelope(buildingUnitAddressWasAttached, position + 2),
                    CreateEnvelope(buildingUnitsAddressesWereReaddressed, position + 3))
                .Then(async context =>
                {
                    var document = await context.BuildingUnitDocuments.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    document.Should().NotBeNull();
                    document!.Document.AddressPersistentLocalIds.Should().Contain(newAddressId);
                    document.Document.AddressPersistentLocalIds.Should().NotContain(buildingUnitAddressWasAttached.AddressPersistentLocalId);
                    document.LastChangedOn.Should().Be(buildingUnitsAddressesWereReaddressed.Provenance.Timestamp);

                    var oldAddressPuris = new List<string>
                    {
                        $"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"
                    };

                    var expectedAddressPuris = oldAddressPuris
                        .Except([$"{AddressNamespace}/{buildingUnitAddressWasAttached.AddressPersistentLocalId}"])
                        .Concat([$"{AddressNamespace}/{newAddressId}"])
                        .Distinct()
                        .ToList();

                    ChangeFeedServiceMock.Verify(x => x.CreateCloudEventWithData(
                            It.IsAny<long>(),
                            buildingUnitsAddressesWereReaddressed.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                            BuildingUnitEventTypes.UpdateV1,
                            buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId.ToString(),
                            It.IsAny<DateTimeOffset>(),
                            It.Is<List<string>>(nisCodes => nisCodes.Contains(NisCode)),
                            It.Is<List<BaseRegistriesCloudEventAttribute>>(attrs =>
                                attrs.Any(a => a.Name == BuildingUnitAttributeNames.AdresIds
                                               && ((List<string>)a.OldValue!).SequenceEqual(oldAddressPuris)
                                               && ((List<string>)a.NewValue!).SequenceEqual(expectedAddressPuris))),
                            BuildingBuildingUnitsAddressesWereReaddressed.EventName,
                            It.IsAny<string>()),
                        Times.Once);
                });
        }

        #region Helpers

        private static void AssertFeedItem(
            BuildingUnitFeedItem? feedItem,
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

        private static async Task<BuildingUnitFeedItem?> FindFeedItemByBuildingUnitPersistentLocalId(FeedContext context, int buildingUnitPersistentLocalId)
        {
            return await context.BuildingUnitFeed
                .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId)
                .FirstOrDefaultAsync();
        }

        private static bool AssertPointList(List<BuildingUnitPositionCloudEventValue> pointList, string gml)
        {
            var lambert72 = pointList.SingleOrDefault(p => p.Projection == SystemReferenceId.SrsNameLambert72);
            lambert72.Should().NotBeNull();

            var lambert08 = pointList.SingleOrDefault(p => p.Projection == SystemReferenceId.SrsNameLambert2008);
            lambert08.Should().NotBeNull();

            pointList.Count.Should().Be(2);
            pointList.Should().Contain(p => p.Gml == gml);

            gml.Should().ContainAny(SystemReferenceId.SrsNameLambert72, SystemReferenceId.SrsNameLambert2008);

            return true;
        }

        private Envelope<T> CreateEnvelope<T>(T @event, long position) where T : IMessage
        {
            var metadata = new Dictionary<string, object>
            {
                { "Position", position },
                { "EventName", @event.GetType().Name },
                { "CommandId", Guid.NewGuid().ToString() }
            };
            return new Envelope<T>(
                new Envelope(@event, metadata));
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
                .Setup(x => x.GetOverlappingNisCodes(It.IsAny<string>(), It.IsAny<Instant>()))
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

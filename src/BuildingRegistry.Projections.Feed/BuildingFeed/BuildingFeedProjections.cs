namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    [ConnectedProjectionName("Feed endpoint gebouw")]
    [ConnectedProjectionDescription("Projectie die de gebouw data voor de gebouw cloudevent feed voorziet.")]
    public class BuildingFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;

        public BuildingFeedProjections(IChangeFeedService changeFeedService)
        {
            _changeFeedService = changeFeedService;

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var buildingStatus = MapStatus(BuildingStatus.Parse(message.Message.BuildingStatus));
                var geometryMethod = MapGeometryMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod));

                var document = new BuildingDocument(
                    message.Message.BuildingPersistentLocalId,
                    buildingStatus,
                    geometryMethod,
                    message.Message.Provenance.Timestamp);

                document.IsRemoved = message.Message.IsRemoved;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);

                await context.BuildingDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, null, document.Document.Status),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, null, document.Document.GeometryMethod),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, null, CreateGeometryValues(geometry))
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingEventTypes.CreateV1);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var document = new BuildingDocument(
                    message.Message.BuildingPersistentLocalId,
                    GebouwStatus.Gepland,
                    GeometrieMethode.Ingeschetst,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);

                await context.BuildingDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, null, GebouwStatus.Gepland),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, null, GeometrieMethode.Ingeschetst),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, null, CreateGeometryValues(geometry))
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingEventTypes.CreateV1);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var document = new BuildingDocument(
                    message.Message.BuildingPersistentLocalId,
                    GebouwStatus.Gerealiseerd,
                    GeometrieMethode.IngemetenGRB,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);

                await context.BuildingDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, null, GebouwStatus.Gerealiseerd),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, null, GeometrieMethode.IngemetenGRB),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, null, CreateGeometryValues(geometry))
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingEventTypes.CreateV1);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldGeometryValues = CreateGeometryValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometryBuilding);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, oldGeometryValues, CreateGeometryValues(geometry))
                ]);
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldGeometryValues = CreateGeometryValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometryBuilding);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, oldGeometryValues, CreateGeometryValues(geometry))
                ]);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.InAanbouw;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.InAanbouw)
                ]);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.Gepland;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.Gepland)
                ]);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.Gerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.Gerealiseerd)
                ]);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.InAanbouw;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.InAanbouw)
                ]);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.NietGerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.NietGerealiseerd)
                ]);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.Gepland;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.Gepland)
                ]);
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldGeometryMethod = document.Document.GeometryMethod;
                var oldGeometryValues = CreateGeometryValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometryBuilding);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);
                document.Document.GeometryMethod = GeometrieMethode.IngemetenGRB;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var attributes = new List<BaseRegistriesCloudEventAttribute>
                {
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, oldGeometryValues, CreateGeometryValues(geometry))
                };

                if (oldGeometryMethod != GeometrieMethode.IngemetenGRB)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, oldGeometryMethod, GeometrieMethode.IngemetenGRB));

                await AddCloudEvent(message, document, context, attributes);
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldGeometryValues = CreateGeometryValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometryBuilding);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, oldGeometryValues, CreateGeometryValues(geometry))
                ]);
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouwStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus, GebouwStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], BuildingEventTypes.DeleteV1);
            });

            #endregion

            #region BuildingUnit

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, []);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, []);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, []);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, []);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, []);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, []);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(DoNothing);
            When<Envelope<BuildingUnitWasRegularized>>(DoNothing);
            When<Envelope<BuildingUnitRegularizationWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasDeregulated>>(DoNothing);
            When<Envelope<BuildingUnitDeregulationWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasRetiredV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingUnitPositionWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasAttachedV2>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedV2>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(DoNothing);

            #endregion
        }

        private static async Task<BuildingDocument> FindDocument(FeedContext context, int persistentLocalId, CancellationToken ct)
        {
            var document = await context.BuildingDocuments.FindAsync([persistentLocalId], cancellationToken: ct);
            if (document is null)
                throw new InvalidOperationException($"Could not find document for building {persistentLocalId}");
            return document;
        }

        private async Task AddCloudEvent<T>(
            Envelope<T> message,
            BuildingDocument document,
            FeedContext context,
            List<BaseRegistriesCloudEventAttribute> attributes,
            string eventType = BuildingEventTypes.UpdateV1)
            where T : IHasProvenance, IMessage
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var page = await context.CalculatePage();
            var buildingFeedItem = new BuildingFeedItem(
                position: message.Position,
                page: page)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.BuildingFeed.AddAsync(buildingFeedItem);
            await context.BuildingFeedItemBuildings.AddAsync(
                new BuildingFeedItemBuilding(buildingFeedItem.Id, document.PersistentLocalId));

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                buildingFeedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.PersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                [],
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            buildingFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private async Task CheckToUpdateCache(int page, FeedContext context)
        {
            await _changeFeedService.CheckToUpdateCacheAsync(
                page,
                context,
                async p =>
                {
                    var localCount = context.BuildingFeed.Local
                        .Count(x => x.Page == page && context.Entry(x).State == EntityState.Added);
                    return await context.BuildingFeed.CountAsync(x => x.Page == p) + localCount - 1;
                });
        }

        private static GebouwStatus MapStatus(BuildingStatus status)
        {
            if (status == BuildingStatus.Planned)
                return GebouwStatus.Gepland;
            if (status == BuildingStatus.UnderConstruction)
                return GebouwStatus.InAanbouw;
            if (status == BuildingStatus.Realized)
                return GebouwStatus.Gerealiseerd;
            if (status == BuildingStatus.Retired)
                return GebouwStatus.Gehistoreerd;
            if (status == BuildingStatus.NotRealized)
                return GebouwStatus.NietGerealiseerd;

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        private static GeometrieMethode MapGeometryMethod(BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingGeometryMethod.Outlined)
                return GeometrieMethode.Ingeschetst;
            if (geometryMethod == BuildingGeometryMethod.MeasuredByGrb)
                return GeometrieMethode.IngemetenGRB;

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }

        private static List<BuildingGeometryCloudEventValue> CreateGeometryValues(Geometry geometry)
        {
            var list = new List<BuildingGeometryCloudEventValue>();
            var gml = geometry.ConvertToGml(false);
            switch (geometry.SRID)
            {
                case SystemReferenceId.SridLambert72:
                {
                    list.Add(new BuildingGeometryCloudEventValue(gml, SystemReferenceId.SrsNameLambert72));

                    var lambert08Geometry = geometry.TransformFromLambert72To08();
                    list.Add(new BuildingGeometryCloudEventValue(lambert08Geometry.ConvertToGml(false), SystemReferenceId.SrsNameLambert2008));
                    break;
                }
                case SystemReferenceId.SridLambert2008:
                {
                    var lambert72Geometry = geometry.TransformFromLambert08To72();
                    list.Add(new BuildingGeometryCloudEventValue(lambert72Geometry.ConvertToGml(false), SystemReferenceId.SrsNameLambert72));
                    list.Add(new BuildingGeometryCloudEventValue(gml, SystemReferenceId.SrsNameLambert2008));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry), geometry, null);
            }

            return list;
        }

        private static Task DoNothing<T>(FeedContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}

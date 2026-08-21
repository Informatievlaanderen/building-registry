namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using Newtonsoft.Json.Serialization;
    using NodaTime;

    [ConnectedProjectionName("Feed endpoint gebouw (cloudevents)")]
    [ConnectedProjectionDescription("Projectie die de gebouw data voor de gebouw cloudevent feed voorziet.")]
    public class BuildingFeedProjections : ConnectedProjection<FeedContext>
    {
        private static readonly CamelCaseNamingStrategy NamingStrategy = new();

        private readonly IChangeFeedService _changeFeedService;
        private readonly IMunicipalityGeometryRepository _municipalityGeometryRepository;

        public BuildingFeedProjections(IChangeFeedService changeFeedService, IMunicipalityGeometryRepository municipalityGeometryRepository)
        {
            _changeFeedService = changeFeedService;
            _municipalityGeometryRepository = municipalityGeometryRepository;

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

                if (document.IsRemoved)
                    return;

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, null, document.Document.Status.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(document.Document.GeometryMethod)),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, null, CreateGeometryValues(geometry))
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingEventTypes.CreateV1);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var document = new BuildingDocument(
                    message.Message.BuildingPersistentLocalId,
                    new GebouwStatus(GebouwStatusValue.Gepland),
                    GebouwGeometrieMethode.Ingeschetst,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);

                await context.BuildingDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, null, document.Document.Status.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(document.Document.GeometryMethod)),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, null, CreateGeometryValues(geometry))
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingEventTypes.CreateV1);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var document = new BuildingDocument(
                    message.Message.BuildingPersistentLocalId,
                    new GebouwStatus(GebouwStatusValue.Gerealiseerd),
                    GebouwGeometrieMethode.IngemetenGRB,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.GeometryAsGml = geometry.ConvertToGml(false);

                await context.BuildingDocuments.AddAsync(document, ct);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, null, document.Document.Status.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(document.Document.GeometryMethod)),
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
                document.Document.Status = new GebouwStatus(GebouwStatusValue.InAanbouw);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouwStatus(GebouwStatusValue.Gepland);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouwStatus(GebouwStatusValue.Gerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouwStatus(GebouwStatusValue.InAanbouw);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouwStatus(GebouwStatusValue.NietGerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouwStatus(GebouwStatusValue.Gepland);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
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
                document.Document.GeometryMethod = GebouwGeometrieMethode.IngemetenGRB;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var attributes = new List<BaseRegistriesCloudEventAttribute>
                {
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.Geometry, oldGeometryValues, CreateGeometryValues(geometry))
                };

                if (oldGeometryMethod != GebouwGeometrieMethode.IngemetenGRB)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.GeometryMethod, ToGeometrieMethodePuri(oldGeometryMethod), ToGeometrieMethodePuri(GebouwGeometrieMethode.IngemetenGRB)));

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
                document.Document.Status = new GebouwStatus(GebouwStatusValue.Gehistoreerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
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

            var nisCodes = GetNisCodes(document.Document.ExtendedWkbGeometry, message.Message.Provenance.Timestamp);

            var page = await context.CalculatePage();
            var buildingFeedItem = new BuildingFeedItem(
                position: message.Position,
                page: page,
                buildingPersistentLocalId: document.BuildingPersistentLocalId)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.BuildingFeed.AddAsync(buildingFeedItem);

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                buildingFeedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.BuildingPersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                nisCodes,
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            buildingFeedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await MarkCompletedPage(page, context);
        }

        private List<string> GetNisCodes(string? extendedWkbGeometry, Instant eventTimestamp)
        {
            if (string.IsNullOrEmpty(extendedWkbGeometry))
                return new List<string>();

            return _municipalityGeometryRepository.GetOverlappingNisCodes(extendedWkbGeometry, eventTimestamp);
        }

        private async Task MarkCompletedPage(int page, FeedContext context)
        {
            await _changeFeedService.MarkCompletedPageAsync(
                page,
                // Committed rows only. Rows that are merely tracked as added on the context must not be
                // counted here, or the cache record can be published for a page that is not yet complete
                // in the database.
                async p => await context.BuildingFeed.CountAsync(x => x.Page == p));
        }

        private static GebouwStatus MapStatus(BuildingStatus status)
        {
            if (status == BuildingStatus.Planned)
                return new GebouwStatus(GebouwStatusValue.Gepland);
            if (status == BuildingStatus.UnderConstruction)
                return new GebouwStatus(GebouwStatusValue.InAanbouw);
            if (status == BuildingStatus.Realized)
                return new GebouwStatus(GebouwStatusValue.Gerealiseerd);
            if (status == BuildingStatus.Retired)
                return new GebouwStatus(GebouwStatusValue.Gehistoreerd);
            if (status == BuildingStatus.NotRealized)
                return new GebouwStatus(GebouwStatusValue.NietGerealiseerd);

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        private static GebouwGeometrieMethode MapGeometryMethod(BuildingGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingGeometryMethod.Outlined)
                return GebouwGeometrieMethode.Ingeschetst;
            if (geometryMethod == BuildingGeometryMethod.MeasuredByGrb)
                return GebouwGeometrieMethode.IngemetenGRB;

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }

        private static string ToGeometrieMethodePuri(GebouwGeometrieMethode geometrieMethode)
            => OsloNamespaces.GebouwGeometrieMethode.ToPuri(NamingStrategy.GetPropertyName(geometrieMethode.ToString(), false));

        private static List<PolygonGeometrie> CreateGeometryValues(Geometry geometry)
        {
            var list = new List<PolygonGeometrie>();
            var gml = geometry.ConvertToGml(false);
            switch (geometry.SRID)
            {
                case SystemReferenceId.SridLambert72:
                {
                    list.Add(new PolygonGeometrie(gml));

                    var lambert08Geometry = geometry.TransformFromLambert72To08();
                    list.Add(new PolygonGeometrie(lambert08Geometry.ConvertToGml(false)));
                    break;
                }
                case SystemReferenceId.SridLambert2008:
                {
                    var lambert72Geometry = geometry.TransformFromLambert08To72();
                    list.Add(new PolygonGeometrie(lambert72Geometry.ConvertToGml(false)));
                    list.Add(new PolygonGeometrie(gml));
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

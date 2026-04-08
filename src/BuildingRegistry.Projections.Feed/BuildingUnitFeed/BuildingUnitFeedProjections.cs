namespace BuildingRegistry.Projections.Feed.BuildingUnitFeed
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
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using BuildingFeed;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    [ConnectedProjectionName("Feed endpoint gebouweenheid (cloudevents)")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheid data voor de gebouweenheid cloudevent feed voorziet.")]
    public class BuildingUnitFeedProjections : ConnectedProjection<FeedContext>
    {
        private readonly IChangeFeedService _changeFeedService;
        private readonly IMunicipalityGeometryRepository _municipalityGeometryRepository;

        public BuildingUnitFeedProjections(IChangeFeedService changeFeedService, IMunicipalityGeometryRepository municipalityGeometryRepository)
        {
            _changeFeedService = changeFeedService;
            _municipalityGeometryRepository = municipalityGeometryRepository;

            #region Building (geometry tracking)

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                if(message.Message.IsRemoved)
                    return;

                await context.BuildingGeometryForBuildingUnit.AddAsync(
                    new BuildingGeometryForBuildingUnit(
                        message.Message.BuildingPersistentLocalId,
                        message.Message.ExtendedWkbGeometry), ct);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var status = MapBuildingUnitStatus(BuildingUnitStatus.Parse(buildingUnit.Status));
                    var function = MapBuildingUnitFunction(BuildingUnitFunction.Parse(buildingUnit.Function));
                    var geometryMethod = MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod));
                    var addressPersistentLocalIds = buildingUnit.AddressPersistentLocalIds.ToList();

                    var document = new BuildingUnitDocument(
                        buildingUnit.BuildingUnitPersistentLocalId,
                        message.Message.BuildingPersistentLocalId,
                        status,
                        function,
                        geometryMethod,
                        message.Message.Provenance.Timestamp);

                    document.IsRemoved = buildingUnit.IsRemoved;
                    document.Document.AddressPersistentLocalIds = addressPersistentLocalIds;
                    document.Document.HasDeviation = false;

                    var geometry = GmlHelpers.ParseGeometry(buildingUnit.ExtendedWkbGeometry);
                    document.Document.ExtendedWkbGeometry = buildingUnit.ExtendedWkbGeometry;
                    document.Document.PositionAsGml = geometry.ConvertToGml(false);

                    await context.BuildingUnitDocuments.AddAsync(document, ct);

                    var buildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                    List<BaseRegistriesCloudEventAttribute> attributes =
                    [
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, document.Document.Status),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, document.Document.Function),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, document.Document.GeometryMethod),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, null, BuildAddressPuris(addressPersistentLocalIds)),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, null, buildingPuri),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, null, false)
                    ];

                    await AddCloudEvent(message, document, context, attributes, BuildingUnitEventTypes.CreateV1);
                }
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                await context.BuildingGeometryForBuildingUnit.AddAsync(
                    new BuildingGeometryForBuildingUnit(
                        message.Message.BuildingPersistentLocalId,
                        message.Message.ExtendedWkbGeometry), ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                await context.BuildingGeometryForBuildingUnit.AddAsync(
                    new BuildingGeometryForBuildingUnit(
                        message.Message.BuildingPersistentLocalId,
                        message.Message.ExtendedWkbGeometry), ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;
            });

            When<Envelope<BuildingWasRemovedV2>>(DoNothing);
            When<Envelope<BuildingBecameUnderConstructionV2>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(DoNothing);
            When<Envelope<BuildingWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(DoNothing);
            When<Envelope<BuildingWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);

            #endregion

            #region BuildingUnit

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryMethod = MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                var function = MapBuildingUnitFunction(BuildingUnitFunction.Parse(message.Message.Function));

                var document = new BuildingUnitDocument(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    GebouweenheidStatus.Gepland,
                    function,
                    geometryMethod,
                    message.Message.Provenance.Timestamp);

                document.Document.HasDeviation = message.Message.HasDeviation;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);

                await context.BuildingUnitDocuments.AddAsync(document, ct);

                var buildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, GebouweenheidStatus.Gepland),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, function),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, geometryMethod),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, null, new List<string>()),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, null, buildingPuri),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, null, message.Message.HasDeviation)
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingUnitEventTypes.CreateV1);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var status = MapBuildingUnitStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus));
                var geometryMethod = MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));

                var document = new BuildingUnitDocument(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    status,
                    GebouweenheidFunctie.GemeenschappelijkDeel,
                    geometryMethod,
                    message.Message.Provenance.Timestamp);

                document.Document.HasDeviation = message.Message.HasDeviation;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);

                await context.BuildingUnitDocuments.AddAsync(document, ct);

                var buildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, status),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, GebouweenheidFunctie.GemeenschappelijkDeel),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, geometryMethod),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, null, new List<string>()),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, null, buildingPuri),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, null, message.Message.HasDeviation)
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingUnitEventTypes.CreateV1);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);

                var oldBuildingPuri = BuildBuildingPuri(document.BuildingPersistentLocalId);
                var newBuildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                document.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;

                var status = MapBuildingUnitStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus));
                var function = MapBuildingUnitFunction(BuildingUnitFunction.Parse(message.Message.Function));
                var geometryMethod = MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                var addressPersistentLocalIds = message.Message.AddressPersistentLocalIds.ToList();

                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                var oldStatus = document.Document.Status;
                var oldFunction = document.Document.Function;
                var oldGeometryMethod = document.Document.GeometryMethod;
                var oldPositionValues = CreatePositionValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));
                var oldHasDeviation = document.Document.HasDeviation;

                document.Document.Status = status;
                document.Document.Function = function;
                document.Document.GeometryMethod = geometryMethod;
                document.Document.AddressPersistentLocalIds = addressPersistentLocalIds;
                document.Document.HasDeviation = message.Message.HasDeviation;
                document.IsRemoved = false;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var newAddressPuris = BuildAddressPuris(addressPersistentLocalIds);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, status),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, oldFunction, function),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, oldGeometryMethod, geometryMethod),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, oldPositionValues, CreatePositionValues(geometry)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, oldBuildingPuri, newBuildingPuri),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, oldHasDeviation, message.Message.HasDeviation)
                ];

                await AddCloudEvent(message, document, context, attributes);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gerealiseerd)
                ]);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gerealiseerd)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gepland;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gepland)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gepland;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gepland)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gerealiseerd)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gepland;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gepland)
                ]);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.NietGerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.NietGerealiseerd)
                ]);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.NietGerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.NietGerealiseerd)
                ]);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.NietGerealiseerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.NietGerealiseerd)
                ]);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = GebouweenheidStatus.Gehistoreerd;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus, GebouweenheidStatus.Gehistoreerd)
                ]);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldGeometryMethod = document.Document.GeometryMethod;
                var oldPositionValues = CreatePositionValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));

                var newGeometryMethod = MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);
                document.Document.GeometryMethod = newGeometryMethod;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var attributes = new List<BaseRegistriesCloudEventAttribute>
                {
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, oldPositionValues, CreatePositionValues(geometry))
                };

                if (oldGeometryMethod != newGeometryMethod)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, oldGeometryMethod, newGeometryMethod));

                await AddCloudEvent(message, document, context, attributes);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], BuildingUnitEventTypes.DeleteV1);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                document.IsRemoved = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context, [], BuildingUnitEventTypes.DeleteV1);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);

                var status = MapBuildingUnitStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus));
                var function = MapBuildingUnitFunction(BuildingUnitFunction.Parse(message.Message.Function));
                var geometryMethod = MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));

                document.IsRemoved = false;
                document.Document.Status = status;
                document.Document.Function = function;
                document.Document.GeometryMethod = geometryMethod;
                document.Document.AddressPersistentLocalIds = new List<int>();
                document.Document.HasDeviation = message.Message.HasDeviation;

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var buildingPuri = BuildBuildingPuri(document.BuildingPersistentLocalId);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, status),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, function),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, geometryMethod),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, null, new List<string>()),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, null, buildingPuri),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, null, message.Message.HasDeviation)
                ];

                await AddCloudEvent(message, document, context, attributes, BuildingUnitEventTypes.CreateV1);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldHasDeviation = document.Document.HasDeviation;
                document.Document.HasDeviation = false;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, oldHasDeviation, false)
                ]);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldHasDeviation = document.Document.HasDeviation;
                document.Document.HasDeviation = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, oldHasDeviation, true)
                ]);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldHasDeviation = document.Document.HasDeviation;
                document.Document.HasDeviation = true;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, oldHasDeviation, true)
                ]);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldHasDeviation = document.Document.HasDeviation;
                document.Document.HasDeviation = false;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, oldHasDeviation, false)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Add(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.Document.AddressPersistentLocalIds.Remove(message.Message.AddressPersistentLocalId);
                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);

                document.Document.AddressPersistentLocalIds.Remove(message.Message.PreviousAddressPersistentLocalId);
                document.Document.AddressPersistentLocalIds.Add(message.Message.NewAddressPersistentLocalId); //this can cause doubles, but we'll build the uri's unique

                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);

                document.Document.AddressPersistentLocalIds.Remove(message.Message.PreviousAddressPersistentLocalId);
                if (!document.Document.AddressPersistentLocalIds.Contains(message.Message.NewAddressPersistentLocalId))
                {
                    document.Document.AddressPersistentLocalIds.Add(message.Message.NewAddressPersistentLocalId);
                }

                var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                ]);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddress in message.Message.BuildingUnitsReaddresses)
                {
                    var document = await FindDocument(context, buildingUnitReaddress.BuildingUnitPersistentLocalId, ct);
                    var oldAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);

                    foreach (var addressPersistentLocalId in buildingUnitReaddress.DetachedAddressPersistentLocalIds)
                    {
                        document.Document.AddressPersistentLocalIds.Remove(addressPersistentLocalId);
                    }

                    foreach (var addressPersistentLocalId in buildingUnitReaddress.AttachedAddressPersistentLocalIds)
                    {
                        if (!document.Document.AddressPersistentLocalIds.Contains(addressPersistentLocalId))
                        {
                            document.Document.AddressPersistentLocalIds.Add(addressPersistentLocalId);
                        }
                    }

                    var newAddressPuris = BuildAddressPuris(document.Document.AddressPersistentLocalIds);
                    document.LastChangedOn = message.Message.Provenance.Timestamp;

                    await AddCloudEvent(message, document, context,
                    [
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris)
                    ]);
                }
            });

            #endregion
        }

        private static async Task<BuildingUnitDocument> FindDocument(FeedContext context, int buildingUnitPersistentLocalId, CancellationToken ct)
        {
            var document = await context.BuildingUnitDocuments.FindAsync([buildingUnitPersistentLocalId], cancellationToken: ct);
            if (document is null)
                throw new InvalidOperationException($"Could not find document for building unit {buildingUnitPersistentLocalId}");
            return document;
        }

        private static async Task<BuildingGeometryForBuildingUnit> FindBuildingGeometry(FeedContext context, int buildingPersistentLocalId, CancellationToken ct)
        {
            var geometry = await context.BuildingGeometryForBuildingUnit.FindAsync([buildingPersistentLocalId], cancellationToken: ct);
            if (geometry is null)
                throw new InvalidOperationException($"Could not find building geometry for building {buildingPersistentLocalId}");
            return geometry;
        }

        private async Task AddCloudEvent<T>(
            Envelope<T> message,
            BuildingUnitDocument document,
            FeedContext context,
            List<BaseRegistriesCloudEventAttribute> attributes,
            string eventType = BuildingUnitEventTypes.UpdateV1)
            where T : IHasProvenance, IMessage
        {
            context.Entry(document).Property(x => x.Document).IsModified = true;

            var nisCodes = GetNisCodes(document.BuildingPersistentLocalId, context, message.Message.Provenance.Timestamp);

            var page = await context.CalculateBuildingUnitPage();
            var feedItem = new BuildingUnitFeedItem(
                position: message.Position,
                page: page,
                buildingUnitPersistentLocalId: document.PersistentLocalId)
            {
                Application = message.Message.Provenance.Application,
                Modification = message.Message.Provenance.Modification,
                Operator = message.Message.Provenance.Operator,
                Organisation = message.Message.Provenance.Organisation,
                Reason = message.Message.Provenance.Reason
            };
            await context.BuildingUnitFeed.AddAsync(feedItem);

            var cloudEvent = _changeFeedService.CreateCloudEventWithData(
                feedItem.Id,
                message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset(),
                eventType,
                document.PersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                nisCodes,
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            feedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await CheckToUpdateCache(page, context);
        }

        private List<string> GetNisCodes(int buildingPersistentLocalId, FeedContext context, Instant eventTimestamp)
        {
            var buildingGeometry = context.BuildingGeometryForBuildingUnit
                .Local
                .SingleOrDefault(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                ?? context.BuildingGeometryForBuildingUnit
                    .SingleOrDefault(x => x.BuildingPersistentLocalId == buildingPersistentLocalId);

            if (buildingGeometry is null || string.IsNullOrEmpty(buildingGeometry.ExtendedWkbGeometry))
                throw new InvalidOperationException($"Could not find building geometry for building {buildingPersistentLocalId}");

            return _municipalityGeometryRepository.GetOverlappingNisCodes(buildingGeometry.ExtendedWkbGeometry, eventTimestamp);
        }

        private async Task CheckToUpdateCache(int page, FeedContext context)
        {
            await _changeFeedService.CheckToUpdateCacheAsync(
                page,
                context,
                async p =>
                {
                    var localCount = context.BuildingUnitFeed.Local
                        .Count(x => x.Page == page && context.Entry(x).State == EntityState.Added);
                    return await context.BuildingUnitFeed.CountAsync(x => x.Page == p) + localCount - 1;
                });
        }

        private static GebouweenheidStatus MapBuildingUnitStatus(BuildingUnitStatus status)
        {
            if (status == BuildingUnitStatus.Planned)
                return GebouweenheidStatus.Gepland;
            if (status == BuildingUnitStatus.Realized)
                return GebouweenheidStatus.Gerealiseerd;
            if (status == BuildingUnitStatus.Retired)
                return GebouweenheidStatus.Gehistoreerd;
            if (status == BuildingUnitStatus.NotRealized)
                return GebouweenheidStatus.NietGerealiseerd;

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        private static GebouweenheidFunctie MapBuildingUnitFunction(BuildingUnitFunction function)
        {
            if (function == BuildingUnitFunction.Common)
                return GebouweenheidFunctie.GemeenschappelijkDeel;
            if (function == BuildingUnitFunction.Unknown)
                return GebouweenheidFunctie.NietGekend;

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        private static PositieGeometrieMethode MapBuildingUnitGeometryMethod(BuildingUnitPositionGeometryMethod geometryMethod)
        {
            if (geometryMethod == BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                return PositieGeometrieMethode.AangeduidDoorBeheerder;
            if (geometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject)
                return PositieGeometrieMethode.AfgeleidVanObject;

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }

        private static List<BuildingUnitPositionCloudEventValue> CreatePositionValues(Geometry geometry)
        {
            var list = new List<BuildingUnitPositionCloudEventValue>();
            var gml = geometry.ConvertToGml(false);
            switch (geometry.SRID)
            {
                case SystemReferenceId.SridLambert72:
                {
                    list.Add(new BuildingUnitPositionCloudEventValue(gml, SystemReferenceId.SrsNameLambert72));

                    var lambert08Geometry = geometry.TransformFromLambert72To08();
                    list.Add(new BuildingUnitPositionCloudEventValue(lambert08Geometry.ConvertToGml(false), SystemReferenceId.SrsNameLambert2008));
                    break;
                }
                case SystemReferenceId.SridLambert2008:
                {
                    var lambert72Geometry = geometry.TransformFromLambert08To72();
                    list.Add(new BuildingUnitPositionCloudEventValue(lambert72Geometry.ConvertToGml(false), SystemReferenceId.SrsNameLambert72));
                    list.Add(new BuildingUnitPositionCloudEventValue(gml, SystemReferenceId.SrsNameLambert2008));
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometry), geometry, null);
            }

            return list;
        }

        private static List<string> BuildAddressPuris(IEnumerable<int> addressPersistentLocalIds)
        {
            return addressPersistentLocalIds
                .Select(id => OsloNamespaces.Adres.ToPuri(id.ToString()))
                .Distinct()
                .ToList();
        }

        private static string BuildBuildingPuri(int buildingPersistentLocalId)
        {
            return OsloNamespaces.Gebouw.ToPuri(buildingPersistentLocalId.ToString());
        }

        private static Task DoNothing<T>(FeedContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}

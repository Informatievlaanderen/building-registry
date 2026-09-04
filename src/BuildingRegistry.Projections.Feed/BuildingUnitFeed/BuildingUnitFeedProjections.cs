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
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building;
    using Building.Events;
    using BuildingFeed;
    using Contract;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using Newtonsoft.Json.Serialization;
    using NodaTime;

    [ConnectedProjectionName("Feed endpoint gebouweenheid (cloudevents)")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheid data voor de gebouweenheid cloudevent feed voorziet.")]
    public class BuildingUnitFeedProjections : ConnectedProjection<FeedContext>
    {
        private static readonly CamelCaseNamingStrategy NamingStrategy = new();

        private readonly IChangeFeedService _changeFeedService;
        private readonly IMunicipalityGeometryRepository _municipalityGeometryRepository;

        public BuildingUnitFeedProjections(IChangeFeedService changeFeedService, IMunicipalityGeometryRepository municipalityGeometryRepository)
        {
            _changeFeedService = changeFeedService;
            _municipalityGeometryRepository = municipalityGeometryRepository;

            #region Building (geometry tracking)

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
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
                        hasDeviation: false,
                        message.Message.Provenance.Timestamp);

                    document.IsRemoved = buildingUnit.IsRemoved;
                    document.Document.AddressPersistentLocalIds = addressPersistentLocalIds;
                    document.Document.HasDeviation = false;

                    var geometry = GmlHelpers.ParseGeometry(buildingUnit.ExtendedWkbGeometry);
                    document.Document.ExtendedWkbGeometry = buildingUnit.ExtendedWkbGeometry;
                    document.Document.PositionAsGml = geometry.ConvertToGml(false);

                    await context.BuildingUnitDocuments.AddAsync(document, ct);

                    if(buildingUnit.IsRemoved)
                        continue;

                    var buildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                    List<BaseRegistriesCloudEventAttribute> attributes =
                    [
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, document.Document.Status.Id),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, document.Document.Function.Id),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(document.Document.GeometryMethod)),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, null, buildingPuri),
                        new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, null, false)
                    ];

                    if(addressPersistentLocalIds.Any())
                        attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, null, BuildAddressPuris(addressPersistentLocalIds)));

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

                await UpdateBuildingUnitsFromBuildingGeometryEvent(
                    context,
                    message,
                    message.Message.BuildingUnitPersistentLocalIds,
                    message.Message.ExtendedWkbGeometryBuildingUnits,
                    ct);
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;

                await UpdateBuildingUnitsFromBuildingGeometryEvent(
                    context,
                    message,
                    message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived),
                    message.Message.ExtendedWkbGeometryBuildingUnits,
                    ct);
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;

                await UpdateBuildingUnitsFromBuildingGeometryEvent(
                    context,
                    message,
                    message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived),
                    message.Message.ExtendedWkbGeometryBuildingUnits,
                    ct);
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var buildingGeometry = await FindBuildingGeometry(context, message.Message.BuildingPersistentLocalId, ct);
                buildingGeometry.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometryBuilding;

                await UpdateBuildingUnitsFromBuildingGeometryEvent(
                    context,
                    message,
                    message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived),
                    message.Message.ExtendedWkbGeometryBuildingUnits,
                    ct);
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
                    new GebouweenheidStatus(GebouweenheidStatusValue.Gepland),
                    function,
                    geometryMethod,
                    message.Message.HasDeviation,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);

                await context.BuildingUnitDocuments.AddAsync(document, ct);

                var buildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, document.Document.Status.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, function.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(geometryMethod)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
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
                    new GebouweenheidFunctie(GebouweenheidFunctieValue.GemeenschappelijkDeel),
                    geometryMethod,
                    message.Message.HasDeviation,
                    message.Message.Provenance.Timestamp);

                var geometry = GmlHelpers.ParseGeometry(message.Message.ExtendedWkbGeometry);
                document.Document.ExtendedWkbGeometry = message.Message.ExtendedWkbGeometry;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);

                await context.BuildingUnitDocuments.AddAsync(document, ct);

                var buildingPuri = BuildBuildingPuri(message.Message.BuildingPersistentLocalId);

                List<BaseRegistriesCloudEventAttribute> attributes =
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, status.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, document.Document.Function.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(geometryMethod)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
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
                var oldExtendedWkbGeometry = document.Document.ExtendedWkbGeometry;
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

                var attributes = new List<BaseRegistriesCloudEventAttribute>();

                if (oldStatus.Id != status.Id)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, status.Id));

                if (oldFunction.Id != function.Id)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, oldFunction.Id, function.Id));

                if (oldGeometryMethod != geometryMethod)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, ToGeometrieMethodePuri(oldGeometryMethod), ToGeometrieMethodePuri(geometryMethod)));

                if (oldExtendedWkbGeometry != message.Message.ExtendedWkbGeometry)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position,
                        CreatePositionValues(GmlHelpers.ParseGeometry(oldExtendedWkbGeometry)), CreatePositionValues(geometry)));

                if (!oldAddressPuris.SequenceEqual(newAddressPuris))
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.AdresIds, oldAddressPuris, newAddressPuris));

                attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GebouwId, oldBuildingPuri, newBuildingPuri));

                if (oldHasDeviation != message.Message.HasDeviation)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.HasDeviation, oldHasDeviation, message.Message.HasDeviation));

                await AddCloudEvent(message, document, context, attributes);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gepland);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gepland);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gepland);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.NietGerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.NietGerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.NietGerealiseerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gehistoreerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
                ]);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var document = await FindDocument(context, message.Message.BuildingUnitPersistentLocalId, ct);
                var oldStatus = document.Document.Status;
                document.Document.Status = new GebouweenheidStatus(GebouweenheidStatusValue.Gehistoreerd);
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                await AddCloudEvent(message, document, context,
                [
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, oldStatus.Id, document.Document.Status.Id)
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
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, ToGeometrieMethodePuri(oldGeometryMethod), ToGeometrieMethodePuri(newGeometryMethod)));

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
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.StatusName, null, status.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Function, null, function.Id),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, null, ToGeometrieMethodePuri(geometryMethod)),
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, null, CreatePositionValues(geometry)),
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
                document.Document.AddressPersistentLocalIds.RemoveAll(id => id == message.Message.AddressPersistentLocalId);
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
                document.Document.AddressPersistentLocalIds.RemoveAll(id => id == message.Message.AddressPersistentLocalId);
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
                document.Document.AddressPersistentLocalIds.RemoveAll(id => id == message.Message.AddressPersistentLocalId);
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
                document.Document.AddressPersistentLocalIds.RemoveAll(id => id == message.Message.AddressPersistentLocalId);
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

                document.Document.AddressPersistentLocalIds.RemoveAll(id => id == message.Message.PreviousAddressPersistentLocalId);
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
                        document.Document.AddressPersistentLocalIds.RemoveAll(id => id == addressPersistentLocalId);
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

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);

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

        private async Task UpdateBuildingUnitsFromBuildingGeometryEvent<T>(
            FeedContext context,
            Envelope<T> message,
            IEnumerable<int> buildingUnitPersistentLocalIds,
            string? extendedWkbGeometryBuildingUnits,
            CancellationToken ct)
            where T : IHasProvenance, IMessage
        {
            var buildingUnitIds = buildingUnitPersistentLocalIds.ToList();
            if (!buildingUnitIds.Any() || string.IsNullOrWhiteSpace(extendedWkbGeometryBuildingUnits))
                return;

            var geometry = GmlHelpers.ParseGeometry(extendedWkbGeometryBuildingUnits);
            var newPositionValues = CreatePositionValues(geometry);
            var newGeometryMethod = PositieGeometrieMethode.AfgeleidVanObject;

            foreach (var buildingUnitPersistentLocalId in buildingUnitIds)
            {
                var document = await FindDocument(context, buildingUnitPersistentLocalId, ct);
                var oldGeometryMethod = document.Document.GeometryMethod;
                var oldPositionValues = CreatePositionValues(GmlHelpers.ParseGeometry(document.Document.ExtendedWkbGeometry));

                document.Document.ExtendedWkbGeometry = extendedWkbGeometryBuildingUnits;
                document.Document.PositionAsGml = geometry.ConvertToGml(false);
                document.Document.GeometryMethod = newGeometryMethod;
                document.LastChangedOn = message.Message.Provenance.Timestamp;

                var attributes = new List<BaseRegistriesCloudEventAttribute>
                {
                    new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.Position, oldPositionValues, newPositionValues)
                };

                if (oldGeometryMethod != newGeometryMethod)
                    attributes.Add(new BaseRegistriesCloudEventAttribute(BuildingUnitAttributeNames.GeometryMethod, ToGeometrieMethodePuri(oldGeometryMethod), ToGeometrieMethodePuri(newGeometryMethod)));

                await AddCloudEvent(message, document, context, attributes);
            }
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
                buildingUnitPersistentLocalId: document.BuildingUnitPersistentLocalId)
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
                document.BuildingUnitPersistentLocalId.ToString(),
                document.LastChangedOnAsDateTimeOffset,
                nisCodes,
                attributes,
                message.EventName,
                message.Metadata["CommandId"].ToString()!);

            feedItem.CloudEventAsString = _changeFeedService.SerializeCloudEvent(cloudEvent);
            await MarkCompletedPage(page, context);
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

        private async Task MarkCompletedPage(int page, FeedContext context)
        {
            await _changeFeedService.MarkCompletedPageAsync(
                page,
                // Committed rows only. Rows that are merely tracked as added on the context must not be
                // counted here, or the cache record can be published for a page that is not yet complete
                // in the database.
                async p => await context.BuildingUnitFeed.CountAsync(x => x.Page == p));
        }

        private static GebouweenheidStatus MapBuildingUnitStatus(BuildingUnitStatus status)
        {
            if (status == BuildingUnitStatus.Planned)
                return new GebouweenheidStatus(GebouweenheidStatusValue.Gepland);
            if (status == BuildingUnitStatus.Realized)
                return new GebouweenheidStatus(GebouweenheidStatusValue.Gerealiseerd);
            if (status == BuildingUnitStatus.Retired)
                return new GebouweenheidStatus(GebouweenheidStatusValue.Gehistoreerd);
            if (status == BuildingUnitStatus.NotRealized)
                return new GebouweenheidStatus(GebouweenheidStatusValue.NietGerealiseerd);

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        private static GebouweenheidFunctie MapBuildingUnitFunction(BuildingUnitFunction function)
        {
            if (function == BuildingUnitFunction.Common)
                return new GebouweenheidFunctie(GebouweenheidFunctieValue.GemeenschappelijkDeel);
            if (function == BuildingUnitFunction.Unknown)
                return new GebouweenheidFunctie(GebouweenheidFunctieValue.NietGekend);

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

        private static List<PointGeometrie> CreatePositionValues(Geometry geometry)
        {
            var list = new List<PointGeometrie>();
            var gml = geometry.ConvertToGml(false);
            switch (geometry.SRID)
            {
                case SystemReferenceId.SridLambert72:
                {
                    list.Add(new PointGeometrie(gml));

                    var lambert08Geometry = geometry.TransformFromLambert72To08(roundingPrecision: 2);
                    list.Add(new PointGeometrie(lambert08Geometry.ConvertToGml(false)));
                    break;
                }
                case SystemReferenceId.SridLambert2008:
                {
                    var lambert72Geometry = geometry.TransformFromLambert08To72();
                    list.Add(new PointGeometrie(lambert72Geometry.ConvertToGml(false)));
                    list.Add(new PointGeometrie(gml));
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

        private static string ToGeometrieMethodePuri(PositieGeometrieMethode geometrieMethode)
            => OsloNamespaces.GebouweenheidGeometrieMethode.ToPuri(NamingStrategy.GetPropertyName(geometrieMethode.ToString(), false));

        private static Task DoNothing<T>(FeedContext context, Envelope<T> envelope, CancellationToken ct) where T : IMessage => Task.CompletedTask;
    }
}

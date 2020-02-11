namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using NodaTime;
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using ValueObjects;
    using Building.Events.Crab;
    using System.Collections.Generic;

    public class BuildingSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public BuildingSyndicationProjections()
        {
            #region Building Events

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                var buildingSyndicationItem = new BuildingSyndicationItem
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    RecordCreatedAt = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    LastChangedOn = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    ChangeType = message.EventName,
                    EventDataAsXml = GetEventDataAsXmlString(message.Message)
                };

                await context
                    .BuildingSyndication
                    .AddAsync(buildingSyndicationItem, ct);
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.IsComplete = true);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.IsComplete = false);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.UnderConstruction);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        x.GeometryMethod = null;
                        x.Geometry = null;
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.PersistentLocalId = message.Message.PersistentLocalId);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.Outlined;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = null);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = null);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.NotRealized);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                RetireUnitsByBuilding(newSyndicationItem.BuildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.Planned);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.Realized);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.Retired);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                RetireUnitsByBuilding(newSyndicationItem.BuildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.UnderConstruction);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.NotRealized);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                RetireUnitsByBuilding(newSyndicationItem.BuildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.Outlined;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.Planned);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.Realized);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.Status = BuildingStatus.Retired);

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                RetireUnitsByBuilding(newSyndicationItem.BuildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => { });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            #endregion Building Events

            #region Building Unit Events

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Common,
                            Version = message.Message.Provenance.Timestamp
                        };

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Unknown,
                            Version = message.Message.Provenance.Timestamp
                        };

                        buildingUnitSyndicationItem.Addresses.Add(new BuildingUnitAddressSyndicationItem
                        {
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Position = message.Position,
                            AddressId = message.Message.AddressId,
                            Count = 1
                        });

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Unknown,
                            Version = message.Message.Provenance.Timestamp,
                            Status = syndicationItem.Status == BuildingStatus.NotRealized ? BuildingUnitStatus.NotRealized : BuildingUnitStatus.Retired
                        };

                        buildingUnitSyndicationItem.Addresses.Add(new BuildingUnitAddressSyndicationItem
                        {
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Position = message.Position,
                            AddressId = message.Message.AddressId,
                            Count = 1
                        });

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Unknown,
                            Version = message.Message.Provenance.Timestamp
                        };

                        buildingUnitSyndicationItem.Addresses.Add(new BuildingUnitAddressSyndicationItem
                        {
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Position = message.Position,
                            AddressId = message.Message.AddressId,
                            Count = 1
                        });

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                        x.BuildingUnits.Remove(x.BuildingUnits.FirstOrDefault(y => y.BuildingUnitId == message.Message.BuildingUnitId)));

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(u => u.BuildingUnitId == message.Message.To);

                        if (unit.Addresses.Any(u => u.AddressId == message.Message.AddressId))
                        {
                            var address = unit.Addresses.Single(u => u.AddressId == message.Message.AddressId);
                            address.Count = address.Count + 1;
                        }
                        else
                        {
                            unit.Addresses.Add(new BuildingUnitAddressSyndicationItem
                            {
                                AddressId = message.Message.AddressId,
                                BuildingUnitId = unit.BuildingUnitId,
                                Count = 1,
                                Position = message.Position,
                            });
                        }

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(u => u.BuildingUnitId == message.Message.From);

                        foreach (var addressId in message.Message.AddressIds)
                        {
                            var addressSyndicationItem = unit.Addresses.SingleOrDefault(u => u.AddressId == addressId);
                            if (addressSyndicationItem != null && addressSyndicationItem.Count > 1)
                                addressSyndicationItem.Count = addressSyndicationItem.Count - 1;
                            else
                                unit.Addresses.Remove(addressSyndicationItem);
                        }

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.IsComplete = true;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.IsComplete = false;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        if (x.BuildingUnits.Any(y => y.BuildingUnitId == message.Message.BuildingUnitId))
                            x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId).PersistentLocalId = message.Message.PersistentLocalId;
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator;
                        unit.PointPosition = message.Message.Position.ToByteArray();
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator;
                        unit.PointPosition = message.Message.Position.ToByteArray();
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        unit.PointPosition = message.Message.Position.ToByteArray();
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        unit.PointPosition = message.Message.Position.ToByteArray();
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = null;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.NotRealized;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Planned;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Realized;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Retired;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.NotRealized;
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.NotRealized;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Planned;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Realized;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Retired;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.Retired;
                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);
                ApplyProvenance(newSyndicationItem, message.Message.Provenance);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x =>
                    {
                        x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId)
                            .Readdresses.Add(new BuildingUnitReaddressSyndicationItem
                            {
                                Position = message.Position,
                                BuildingUnitId = message.Message.BuildingUnitId,
                                OldAddressId = message.Message.OldAddressId,
                                NewAddressId = message.Message.NewAddressId,
                                ReaddressBeginDate = message.Message.BeginDate
                            });
                    });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => { });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) =>
            {
                var syndicationItem = await context.LatestPosition(message.Message.BuildingId, ct);

                if (syndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.BuildingId);

                if (syndicationItem.Position >= message.Position)
                    return;

                var newSyndicationItem = syndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => { });

                newSyndicationItem.EventDataAsXml = GetEventDataAsXmlString(message.Message);

                await context
                    .BuildingSyndication
                    .AddAsync(newSyndicationItem, ct);
            });

            #endregion

            //CRAB
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static string GetEventDataAsXmlString<T>(T message)
        {
            return message.ToXml(message.GetType().Name).ToString(SaveOptions.DisableFormatting);
        }

        private static ProjectionItemNotFoundException<BuildingSyndicationProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<BuildingSyndicationProjections>(addressId.ToString("D"));

        private static void ApplyProvenance(BuildingSyndicationItem item, ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Reason = provenance.Reason;
        }

        private static void ApplyUnitVersion(BuildingUnitSyndicationItem item, Instant version)
        {
            item.Version = version;
        }

        private static void RetireUnitsByBuilding(
            IEnumerable<BuildingUnitSyndicationItem> buildingUnits,
            ICollection<Guid> buildingUnitIdsToNotRealize,
            ICollection<Guid> buildingUnitIdsToRetire,
            Instant version,
            LegacyContext context)
        {
            foreach (var buildingUnitDetailItem in buildingUnits)
            {
                if (buildingUnitIdsToNotRealize.Contains(buildingUnitDetailItem.BuildingUnitId))
                    buildingUnitDetailItem.Status = BuildingUnitStatus.NotRealized;
                else if (buildingUnitIdsToRetire.Contains(buildingUnitDetailItem.BuildingUnitId))
                    buildingUnitDetailItem.Status = BuildingUnitStatus.Retired;

                buildingUnitDetailItem.Addresses.Clear();

                ApplyUnitVersion(buildingUnitDetailItem, version);
            }
        }

        private static void DoNothing() { }
    }
}

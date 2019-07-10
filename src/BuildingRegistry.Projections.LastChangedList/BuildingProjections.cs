namespace BuildingRegistry.Projections.LastChangedList
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;

    public class BuildingProjections : LastChangedListConnectedProjection
    {
        protected override string CacheKeyFormat => "legacy/building:{{0}}.{1}";
        protected override string UriFormat => "/v1/gebouwen/{{0}}";

        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml };

        public BuildingProjections()
            : base(SupportedAcceptTypes)
        {
            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct);

                foreach (var record in attachedRecords)
                {
                    record.CacheKey = string.Format(record.CacheKey, message.Message.PersistentLocalId);
                    record.Uri = string.Format(record.Uri, message.Message.PersistentLocalId);
                }
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingId.ToString(), message.Position, context, ct));
        }
    }
}

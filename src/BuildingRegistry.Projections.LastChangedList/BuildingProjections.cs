namespace BuildingRegistry.Projections.LastChangedList
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;

    [ConnectedProjectionName(ProjectionName)]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel gebouwen de gecachte data nog geüpdated moeten worden.")]
    public class BuildingProjections : LastChangedListConnectedProjection
    {
        public const string ProjectionName = "Cache markering gebouwen";
        private static readonly AcceptType[] SupportedAcceptTypes = [AcceptType.JsonLd];

        public BuildingProjections(ICacheValidator cacheValidator)
            : base(SupportedAcceptTypes, cacheValidator)
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            // Building Units
            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingPersistentLocalId.ToString(), message.Position, context, ct));
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.JsonLd => $"oslo/building:{identifier}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.JsonLd => $"/v2/gebouwen/{identifier}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }
    }
}

namespace BuildingRegistry.Projections.LastChangedList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList.Model;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;

    [ConnectedProjectionName(ProjectionName)]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel gebouweenheden de gecachte data nog geüpdated moeten worden.")]
    public class BuildingUnitProjectionsV3 : LastChangedListConnectedProjection
    {
        public const string ProjectionName = "Cache markering gebouweenheden (v3)";
        private static readonly AcceptType[] SupportedAcceptTypes = [AcceptType.JsonLd];

        public BuildingUnitProjectionsV3(ICacheValidator cacheValidator)
            : base(SupportedAcceptTypes, cacheValidator)
        {
            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var attachedRecords =
                        await GetLastChangedRecordsAndUpdatePosition(buildingUnit.BuildingUnitId.ToString(), message.Position, context, ct);

                    context.LastChangedList.RemoveRange(attachedRecords);

                    var records =
                        await GetLastChangedRecordsAndUpdatePosition(buildingUnit.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                    RebuildKeyAndUri(records, buildingUnit.BuildingUnitPersistentLocalId);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId
                         in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await GetLastChangedRecordsAndUpdatePosition(buildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId
                         in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await GetLastChangedRecordsAndUpdatePosition(buildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(buildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await GetLastChangedRecordsAndUpdatePosition(buildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<BuildingWasPlannedV2>>(DoNothing);
            When<Envelope<BuildingBecameUnderConstructionV2>>(DoNothing);
            When<Envelope<BuildingWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingWasRemovedV2>>(DoNothing);
            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(DoNothing);
            #endregion Building

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    await GetLastChangedRecordsAndUpdatePosition(buildingUnitReaddresses.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                }
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.BuildingUnitPersistentLocalId);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var records = await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
                RebuildKeyAndUri(records, message.Message.BuildingUnitPersistentLocalId);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitPersistentLocalId.ToString(), message.Position, context, ct);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);
        }

        private static void RebuildKeyAndUri(IEnumerable<LastChangedRecord>? attachedRecords, int persistentLocalId)
        {
            if (attachedRecords == null)
            {
                return;
            }

            foreach (var record in attachedRecords)
            {
                if (record.CacheKey != null)
                {
                    record.CacheKey = string.Format(record.CacheKey, persistentLocalId);
                }

                if (record.Uri != null)
                {
                    record.Uri = string.Format(record.Uri, persistentLocalId);
                }
            }
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.JsonLd => $"oslo-v3/buildingunit:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.JsonLd => "/v3/gebouweenheden/{0}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static Task DoNothing<T>(LastChangedListContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}

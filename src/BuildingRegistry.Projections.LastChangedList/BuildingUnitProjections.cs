namespace BuildingRegistry.Projections.LastChangedList
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Legacy.Events;
    using Legacy.Events.Crab;

    [ConnectedProjectionName("Cache markering gebouwen en gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die markeert voor hoeveel gebouwen en gebouweenheden de gecachte data nog geüpdated moeten worden.")]
    public class BuildingUnitProjections : LastChangedListConnectedProjection
    {
        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml, AcceptType.JsonLd };

        public BuildingUnitProjections()
            : base(SupportedAcceptTypes)
        {
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct);

                foreach (var record in attachedRecords)
                {
                    if (record.CacheKey is not null)
                    {
                        record.CacheKey = string.Format(record.CacheKey, message.Message.PersistentLocalId);
                    }

                    if (record.Uri is not null)
                    {
                        record.Uri = string.Format(record.Uri, message.Message.PersistentLocalId);
                    }
                }
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.To.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.From.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
                await GetLastChangedRecordsAndUpdatePosition(message.Message.BuildingUnitId.ToString(), message.Position, context, ct));

            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => await DoNothing());

            // Building
            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasRetired>>(async (context, message, ct) => await DoNothing());


            // CRAB
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
        }

        protected override string BuildCacheKey(AcceptType acceptType, string identifier)
        {
            var shortenedAcceptType = acceptType.ToString().ToLowerInvariant();
            return acceptType switch
            {
                AcceptType.Json => $"legacy/buildingunit:{{0}}.{shortenedAcceptType}",
                AcceptType.Xml => $"legacy/buildingunit:{{0}}.{shortenedAcceptType}",
                AcceptType.JsonLd => $"oslo/buildingunit:{{0}}.{shortenedAcceptType}",
                _ => throw new NotImplementedException($"Cannot build CacheKey for type {typeof(AcceptType)}")
            };
        }

        protected override string BuildUri(AcceptType acceptType, string identifier)
        {
            return acceptType switch
            {
                AcceptType.Json => $"/v1/gebouweenheden/{{{0}}}",
                AcceptType.Xml => $"/v1/gebouweenheden/{{{0}}}",
                AcceptType.JsonLd => $"/v2/gebouweenheden/{{{0}}}",
                _ => throw new NotImplementedException($"Cannot build Uri for type {typeof(AcceptType)}")
            };
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}

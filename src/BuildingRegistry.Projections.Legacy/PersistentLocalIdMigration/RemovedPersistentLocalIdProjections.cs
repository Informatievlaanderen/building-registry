namespace BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using Building.Events.Crab;

    public class RemovedPersistentLocalIdProjections : ConnectedProjection<LegacyContext>
    {
        public RemovedPersistentLocalIdProjections()
        {
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) =>
            {
                var id = await context.RemovedPersistentLocalIds.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);

                if(id != null)
                    return;

                await context
                    .RemovedPersistentLocalIds
                    .AddAsync(
                        new RemovedPersistentLocalId
                        {
                            PersistentLocalId = message.Message.PersistentLocalId,
                            Reason = message.Message.Reason,
                            BuildingId = message.Message.BuildingId
                        },
                        ct);
            });

            //Building
            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRetired>>(async (context, message, ct) => DoNothing());

            //BuildingUnit
            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) => DoNothing());
            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) => DoNothing());

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

        private static void DoNothing() { }
    }
}

namespace BuildingRegistry.Producer
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Building.Events;
    using Extensions;

    public abstract class V2Producer : ConnectedProjection<ProducerContext>
    {
        protected abstract Task Produce<T>(int persistentLocalId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage;

        protected V2Producer()
        {
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingBecameUnderConstructionV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingOutlineWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasAttachedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitPositionWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitRemovalWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasDeregulated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitDeregulationWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasNotRealizedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasPlannedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRealizedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRegularized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitRegularizationWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRetiredV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasNotRealizedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasPlannedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasRealizedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<CommonBuildingUnitWasAddedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasMeasured>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingMeasurementWasCorrected>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasDemolished>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingGeometryWasImportedFromGrb>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingMeasurementWasChanged>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasMovedIntoBuilding>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingPersistentLocalId, message.Message.ToContract(), message.Position, ct);
            });
        }
    }
}

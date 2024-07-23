namespace BuildingRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Extensions;
    using Legacy = Legacy.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : V2Producer
    {
        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer)
        {
            _producer = producer;

            #region Legacy Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingBecameIncomplete>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingBecameUnderConstruction>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingGeometryWasRemoved>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingMeasurementByGrbWasCorrected>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingOutlineWasCorrected>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingPersistentLocalIdWasAssigned>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingStatusWasCorrectedToRemoved>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingStatusWasRemoved>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitAddressWasAttached>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitAddressWasDetached>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitBecameComplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitBecameIncomplete>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPersistentLocalIdWasAssigned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPersistentLocalIdWasDuplicated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPersistentLocalIdWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPositionWasAppointedByAdministrator>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitPositionWasDerivedFromObject>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitStatusWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasAdded>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasAddedToRetiredBuilding>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasCorrectedToNotRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasCorrectedToPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasCorrectedToRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasCorrectedToRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasNotRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasNotRealizedByParent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasReaddedByOtherUnitRemoval>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasReaddressed>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingUnitWasRetiredByParent>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasCorrectedToNotRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasCorrectedToPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasCorrectedToRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasCorrectedToRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasCorrectedToUnderConstruction>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasMarkedAsMigrated>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasMeasuredByGrb>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasNotRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasOutlined>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasPlanned>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRealized>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRegistered>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRemoved>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRetired>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.CommonBuildingUnitWasAdded>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            #endregion
        }

        private async Task Produce<T>(string id, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(id),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private async Task Produce<T>(Guid buildingId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            await Produce(buildingId.ToString("D"), message, storePosition, cancellationToken);
        }

        protected override async Task Produce<T>(int persistentLocalId, T message, long storePosition, CancellationToken cancellationToken = default)
        {
            await Produce(persistentLocalId.ToString(), message, storePosition, cancellationToken);
        }
    }
}

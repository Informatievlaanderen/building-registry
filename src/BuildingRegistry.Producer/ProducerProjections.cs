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
    using BuildingDomain = Building.Events;
    using Legacy = Legacy.Events;

    [ConnectedProjectionName("Kafka producer")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "Topic";

        private readonly IProducer _producer;

        public ProducerProjections(IProducer producer)
        {
            _producer = producer;

            #region Legacy Events
            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingAddressWasAttached>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingAddressWasDetached>>(async (_, message, ct) =>
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

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasMarkedAsMigrated>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRealized>>(async (_, message, ct) =>
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

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRecovered>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<Legacy.BuildingWasRegistered>>(async (_, message, ct) =>
            {
                   await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });
            #endregion

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingDomain.BuildingAddressWasAttachedV2>>(async (_, message, ct) =>
            {
                await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingDomain.BuildingAddressWasDetachedV2>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingDomain.BuildingAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingDomain.BuildingAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingDomain.BuildingAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingDomain.BuildingWasMigrated>>(async (_, message, ct) =>
            {
               await Produce(message.Message.BuildingId, message.Message.ToContract(), message.Position, ct);
            });
        }

        private async Task Produce<T>(Guid buildingId, T message, long storePosition, CancellationToken cancellationToken = default)
            where T : class, IQueueMessage
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(buildingId.ToString("D")),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }
    }
}

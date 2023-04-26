namespace BuildingRegistry.Producer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    [ConnectedProjectionName("Kafka producer start from migrate")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt startende vanaf migratie.")]
    public class ProducerMigrateProjections : V2Producer
    {
        public const string TopicKey = "MigrationTopic";

        private readonly IProducer _producer;

        public ProducerMigrateProjections(IProducer producer)
        {
            _producer = producer;
        }

        protected override async Task Produce<T>(int persistentLocalId, T message, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.ProduceJsonMessage(
                new MessageKey(persistentLocalId.ToString()),
                message,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, storePosition.ToString()) },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
        }
    }
}

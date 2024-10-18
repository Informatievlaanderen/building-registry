namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public abstract class BuildingSnapshotReproducer : SnapshotReproducer
    {
        private readonly string _integrationConnectionString;

        public BuildingSnapshotReproducer(
            string integrationConnectionString,
            IOsloProxy osloProxy,
            IProducer producer,
            ILoggerFactory loggerFactory)
            : base(osloProxy, producer, loggerFactory)
        {
            _integrationConnectionString = integrationConnectionString;
        }

        protected override List<(int PersistentLocalId, long Position)> GetIdsToProcess()
        {
            var connection = new NpgsqlConnection(_integrationConnectionString);
        }
    }

    public abstract class SnapshotReproducer : BackgroundService
    {
        private readonly IOsloProxy _osloProxy;
        private readonly IProducer _producer;
        private readonly ILogger<SnapshotReproducer> _logger;

        public SnapshotReproducer(
            IOsloProxy osloProxy,
            IProducer producer,
            ILoggerFactory loggerFactory)
        {
            _osloProxy = osloProxy;
            _producer = producer;

            _logger = loggerFactory.CreateLogger<SnapshotReproducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if(DateTime.Now.Hour == 1)
                {
                    //execute query
                    var idsToProcess = GetIdsToProcess();

                    //reproduce
                    foreach (var building in idsToProcess)
                    {
                            await FindAndProduce(async () =>
                                    await _osloProxy.GetSnapshot(building.PersistentLocalId.ToString(), stoppingToken),
                                    building.Position,
                                    stoppingToken);
                    }

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        protected async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, long storePosition, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.Identificator.ObjectId, result.JsonContent, storePosition, ct);
            }
        }

        protected async Task Produce(string puri, string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        protected abstract List<(int PersistentLocalId, long Position)> GetIdsToProcess();
    }
}

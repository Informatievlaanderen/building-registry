namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Dapper;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using Npgsql;

    public class BuildingSnapshotReproducer : SnapshotReproducer
    {
        private readonly string _integrationConnectionString;

        public BuildingSnapshotReproducer(
            string integrationConnectionString,
            IOsloProxy osloProxy,
            IProducer producer,
            IClock clock,
            ILoggerFactory loggerFactory)
            : base(osloProxy, producer, clock, loggerFactory)
        {
            _integrationConnectionString = integrationConnectionString;
        }

        protected override List<(int PersistentLocalId, long Position)> GetIdsToProcess(DateTimeOffset now)
        {
            var connection = new NpgsqlConnection(_integrationConnectionString);

            var todayMidnight = now.Subtract(now.TimeOfDay);
            var yesterdayMidnight = todayMidnight.AddDays(-1);

            //TODO-rik test op STG 2019-10-06 01:05:31.527 +0200 buildingid 6355606 pos 43
            var records = connection.Query<Record>(
                """
                SELECT building_persistent_local_id, position, version_timestamp
                FROM integration_building.building_versions
                where version_timestamp >= :MinimumDate and version_timestamp < :MaximumDate
                """,
                new
                {
                    MinimumDate = yesterdayMidnight,
                    MaximumDate = todayMidnight
                });

            var duplicateEvents = records
                .GroupBy(x => x.version_timestamp.ToString("yyyyMMddHHmmss"))
                .Where(x => x.Count() > 1)
                .SelectMany(x => x)
                .ToList();

            return duplicateEvents
                .Select(x => (x.building_persistent_local_id, x.position))
                .ToList();
        }

        private sealed record Record(int building_persistent_local_id, long position, DateTimeOffset version_timestamp);
    }

    public abstract class SnapshotReproducer : BackgroundService
    {
        private readonly IOsloProxy _osloProxy;
        private readonly IProducer _producer;
        private readonly IClock _clock;
        private readonly ILogger<SnapshotReproducer> _logger;

        public SnapshotReproducer(
            IOsloProxy osloProxy,
            IProducer producer,
            IClock clock,
            ILoggerFactory loggerFactory)
        {
            _osloProxy = osloProxy;
            _producer = producer;
            _clock = clock;

            _logger = loggerFactory.CreateLogger<SnapshotReproducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = _clock.GetCurrentInstant().ToBelgianDateTimeOffset();
                if (now.Hour == 1)
                {
                    try
                    {
                        //execute query
                        var idsToProcess = GetIdsToProcess(now);

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
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);

                        //TODO-rik notification
                    }
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

        protected abstract List<(int PersistentLocalId, long Position)> GetIdsToProcess(DateTimeOffset now);
    }
}

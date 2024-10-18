namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Dapper;
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

        protected override List<(int PersistentLocalId, long Position)> GetIdsToProcess(DateTime utcNow)
        {
            var connection = new NpgsqlConnection(_integrationConnectionString);

            var todayMidnight = utcNow.Date;
            var yesterdayMidnight = todayMidnight.AddDays(-1);

            var records = connection.Query<BuildingPosition>(
                $"""
                SELECT building_persistent_local_id, position, version_timestamp
                FROM integration_building.building_versions
                where version_timestamp >= '{yesterdayMidnight:yyyy-MM-dd}' and version_timestamp < '{todayMidnight:yyyy-MM-dd}'
                """);

            var duplicateEvents = records
                .GroupBy(x => x.version_timestamp.ToString("yyyyMMddHHmmss"))
                .Where(x => x.Count() > 1)
                .SelectMany(x => x)
                .ToList();

            return duplicateEvents
                .Select(x => (x.building_persistent_local_id, x.position))
                .ToList();
        }

        private sealed class BuildingPosition
        {
            public int building_persistent_local_id { get; init; }
            public long position { get; init; }
            public DateTimeOffset version_timestamp { get; init; }
        }
    }
}

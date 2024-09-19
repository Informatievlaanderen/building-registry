namespace BuildingRegistry.Cache.Invalidator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Building;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;

    public interface IRedisCacheInvalidateService
    {
        Task Invalidate(IEnumerable<BuildingPersistentLocalId> buildingPersistentLocalIds);
    }

    internal sealed class RedisCacheInvalidateService: IRedisCacheInvalidateService
    {
        private readonly string[] _cacheKeyFormats;
        private readonly string _connectionString;

        public RedisCacheInvalidateService(IConfiguration configuration)
        {
            _cacheKeyFormats = configuration.GetValue<string[]>("RedisCacheKeyFormats")
                ?? throw new ArgumentException("No 'RedisCacheKeyFormats' configuration found");
            _connectionString = configuration.GetConnectionString("LastChangedList")
                ?? throw new ArgumentException("No connectionstring 'LastChangedList' found");
        }

        public async Task Invalidate(IEnumerable<BuildingPersistentLocalId> buildingPersistentLocalIds)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var cacheKeys = buildingPersistentLocalIds
                .SelectMany(buildingPersistentLocalId => _cacheKeyFormats.Select(format => string.Format(format, buildingPersistentLocalId)))
                .Distinct()
                .ToList();

            var batches = cacheKeys.SplitIntoBatches(200);
            foreach(var batchedCacheKeys in batches)
            {
                var query = $"""
                             UPDATE [Redis].[LastChangedList]
                             SET [LastPopulatedPosition] = -1
                             WHERE [CacheKey] IN ('{string.Join("','", batchedCacheKeys)}')
                             """;
                connection.Execute(query);
            }
        }
    }
}

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
    using Microsoft.Extensions.Logging;

    public interface IRedisCacheInvalidateService
    {
        Task Invalidate(ICollection<BuildingPersistentLocalId> buildingPersistentLocalIds);
    }

    internal sealed class RedisCacheInvalidateService: IRedisCacheInvalidateService
    {
        private readonly string[] _cacheKeyFormats;
        private readonly string _connectionString;
        private readonly ILogger<RedisCacheInvalidateService> _logger;

        public RedisCacheInvalidateService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _cacheKeyFormats = configuration.GetSection("RedisCacheKeyFormats")
                    .GetChildren()
                    .Select(c => c.Value!)
                    .ToArray();

            if (_cacheKeyFormats.Length == 0)
            {
                throw new ArgumentException("No 'RedisCacheKeyFormats' configuration found");
            }

            _connectionString = configuration.GetConnectionString("LastChangedList")
                ?? throw new ArgumentException("No connectionstring 'LastChangedList' found");
            _logger = loggerFactory.CreateLogger<RedisCacheInvalidateService>();
        }

        public async Task Invalidate(ICollection<BuildingPersistentLocalId> buildingPersistentLocalIds)
        {
            _logger.LogInformation("Invalidating cache for {BuildingPersistentLocalIds}", buildingPersistentLocalIds.Count);

            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var cacheKeys = buildingPersistentLocalIds
                .SelectMany(buildingPersistentLocalId => _cacheKeyFormats.Select(format => string.Format(format, buildingPersistentLocalId)))
                .Distinct()
                .ToList();

            var batches = cacheKeys.SplitBySize(200);
            foreach(var batchedCacheKeys in batches)
            {
                var query = $"""
                             UPDATE [Redis].[LastChangedList]
                             SET [LastPopulatedPosition] = -1
                             WHERE [CacheKey] IN ('{string.Join("','", batchedCacheKeys)}')
                             """;

                _logger.LogInformation("Executing Query: {Query}", query);
                var result = connection.Execute(query);
                _logger.LogInformation("Query executed, affected rows: {Result}", result);
            }
        }
    }
}

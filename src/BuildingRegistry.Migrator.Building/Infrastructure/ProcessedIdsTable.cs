namespace BuildingRegistry.Migrator.Building.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;

    public class ProcessedIdsTable
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly ILogger<ProcessedIdsTable> _logger;

        private string Table => $"[{Schema.MigrateBuilding}].[{_tableName}]";

        public ProcessedIdsTable(string connectionString, string tableName, ILoggerFactory loggerFactory)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _logger = loggerFactory.CreateLogger<ProcessedIdsTable>();
        }

        public async Task<bool> IsMigrated(int idInternal)
        {
            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteScalarAsync<int>($@"
                SELECT count(*)
                FROM {Table}
                WHERE Id = {idInternal}
");
            return result == 1;
        }

        public async Task CreateTableIfNotExists()
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(@$"
IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'{Schema.MigrateBuilding}')
    EXEC('CREATE SCHEMA [{Schema.MigrateBuilding}]');

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{_tableName}' and xtype='U')
CREATE TABLE {Table}(
[Id] [int] NOT NULL,
[IsPageCompleted] [bit] NOT NULL DEFAULT 0,
CONSTRAINT [PK_{_tableName}] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))");
        }

        public async Task Add(int internalId)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.ExecuteAsync(@$"INSERT INTO {Table} (Id, IsPageCompleted) VALUES ('{internalId}', 0)");
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to add Id '{internalId}' to ProcessedIds table");
                throw;
            }
        }

        public async Task CompletePageAsync (IEnumerable<int> processedIds)
        {
            string query = $"UPDATE {Table} SET IsPageCompleted = 1 WHERE Id IN @processedIds;";

            await using var conn = new SqlConnection(_connectionString);

            await conn.ExecuteAsync(query, new { processedIds = processedIds.ToArray() });
        }

        public async Task<IEnumerable<(int processedId, bool isPageCompleted)>> GetProcessedIds()
        {
            await using var conn = new SqlConnection(_connectionString);
            var result = await conn.QueryAsync<(int, bool)>($"SELECT Id, IsPageCompleted FROM {Table} ORDER BY Id desc");
            return result;
        }
    }
}

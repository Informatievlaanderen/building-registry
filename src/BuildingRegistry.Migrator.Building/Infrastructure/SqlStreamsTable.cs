namespace BuildingRegistry.Migrator.Building.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class SqlStreamsTable
    {
        private readonly string _connectionString;
        private readonly int _pageSize;

        public SqlStreamsTable(string connectionString, int pageSize = 500)
        {
            _connectionString = connectionString;
            _pageSize = pageSize;
        }

        public async Task<IEnumerable<(int internalId, string aggregateId)>> ReadNextBuildingStreamPage(int lastCursorPosition)
        {
            await using var conn = new SqlConnection(_connectionString);

            return await conn.QueryAsync<(int, string)>($@"
select top ({_pageSize}) 
	[IdInternal]
    ,[IdOriginal]
from
    [{Schema.Default}].[Streams]
where
    IdOriginal not like 'building-%'
    and IdInternal > {lastCursorPosition}
order by
    IdInternal", commandTimeout: 60);
        }

        public async Task<long> GetStartingMigrationPosition()
        {
            var query = $@"DECLARE @pos bigint;

SELECT @pos = MIN(position) 
FROM [{Schema.Default}].[Streams]
WHERE IdOriginal LIKE 'building-%'

SELECT MIN(position)-1
FROM [{Schema.Default}].[Messages]
WHERE StreamIdInternal IN (
	SELECT IdInternal FROM [{Schema.Default}].[Streams]
	WHERE IdOriginal LIKE 'building-%') AND Position <= @pos";

            await using var conn = new SqlConnection(_connectionString);
            var command = new CommandDefinition(query, conn, commandTimeout: 60 * 60); //can take a while to query 200M+ events..
            return await conn.ExecuteScalarAsync<long>(command);
        }
    }
}

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
            var query = $@"SELECT min(position)-1
                            FROM [{Schema.Default}].[Streams]
                            WHERE IdOriginal like 'building%'";

            await using var conn = new SqlConnection(_connectionString);
            return await conn.ExecuteScalarAsync<long>(query);
        }
    }
}

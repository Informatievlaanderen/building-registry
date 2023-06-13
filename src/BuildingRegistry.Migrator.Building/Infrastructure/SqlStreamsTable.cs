namespace BuildingRegistry.Migrator.Building.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public class SqlBigStreamsTable : SqlStreamsTable
    {
        public SqlBigStreamsTable(string connectionString, int pageSize = 6)
            : base(connectionString, pageSize)
        { }

        public override async Task<IEnumerable<(int internalId, string aggregateId)>> ReadNextStreamPage(int lastCursorPosition)
        {
            await using var conn = new SqlConnection(ConnectionString);

            return await conn.QueryAsync<(int, string)>($@"
select top ({PageSize})
	[IdInternal]
    ,[IdOriginal]
from
    [{Schema.Default}].[Streams]
where
    IdOriginal not like 'building-%'
    and IdInternal > {lastCursorPosition} and version >= {BigBuildingVersionThreshold}
order by
    IdInternal", commandTimeout: 60);
        }
    }

    public class SqlSmallStreamsTable : SqlStreamsTable
    {
        public SqlSmallStreamsTable(string connectionString, int pageSize = 1000)
            : base(connectionString, pageSize)
        { }

        public override async Task<IEnumerable<(int internalId, string aggregateId)>> ReadNextStreamPage(int lastCursorPosition)
        {
            await using var conn = new SqlConnection(ConnectionString);

            return await conn.QueryAsync<(int, string)>($@"
select top ({PageSize})
	[IdInternal]
    ,[IdOriginal]
from
    [{Schema.Default}].[Streams]
where
    IdOriginal not like 'building-%'
    and IdInternal > {lastCursorPosition} and version < {BigBuildingVersionThreshold}
order by
    IdInternal", commandTimeout: 60);
        }
    }

    public abstract class SqlStreamsTable
    {
        protected readonly string ConnectionString;
        protected readonly int PageSize;
        protected const int BigBuildingVersionThreshold = 30000;

        public SqlStreamsTable(string connectionString, int pageSize = 500)
        {
            ConnectionString = connectionString;
            PageSize = pageSize;
        }

        public abstract Task<IEnumerable<(int internalId, string aggregateId)>> ReadNextStreamPage(int lastCursorPosition);

        public static async Task<long> GetStartingMigrationPosition(string connectionString)
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

            await using var conn = new SqlConnection(connectionString);
            var command = new CommandDefinition(query, conn, commandTimeout: 60 * 60); //can take a while to query 200M+ events..
            return await conn.ExecuteScalarAsync<long>(command);
        }
    }
}

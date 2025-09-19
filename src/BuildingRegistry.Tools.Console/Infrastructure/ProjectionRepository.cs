namespace BuildingRegistry.Tools.Console.Infrastructure
{
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using SqlStreamStore;

    public sealed class ProjectionRepository
    {
        private readonly string _eventsConnectionString;
        private readonly IReadonlyStreamStore _streamStore;

        public ProjectionRepository(string eventsConnectionString, IReadonlyStreamStore streamStore)
        {
            _eventsConnectionString = eventsConnectionString;
            _streamStore = streamStore;
        }

        public async Task<long> GetMaxAllStreamPosition()
            => await _streamStore.ReadHeadPosition();

        public async Task<long> GetProducerPosition()
        {
            await using var sql = new Microsoft.Data.SqlClient.SqlConnection(_eventsConnectionString);
            return await sql.ExecuteScalarAsync<long>($@"
SELECT TOP 1 COALESCE([Position], 0)
FROM (
SELECT [Position]
FROM [{Schema.Producer}].[ProjectionStates]
UNION
SELECT [Position]
FROM [{Schema.ProducerSnapshotOslo}].[ProjectionStates]
) as q
ORDER BY [Position] ASC");
        }
    }
}

namespace BuildingRegistry.Tools.Console.CorrectUnitPosition
{
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Data.SqlClient;
    using Dapper;

    public sealed class Repository
    {
        private readonly string _connectionString;

        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task EnsureSchemaAndTablesExist()
        {
            await using var sql = new SqlConnection(_connectionString);

            await sql.ExecuteAsync($"CREATE SCHEMA {Schema.Tools}");

            await sql.ExecuteAsync($"CREATE TABLE {Schema.Tools}.ProcessedBuildings (Id INT NOT NULL PRIMARY KEY IDENTITY, BuildingId INT NOT NULL)");
        }

        public async Task FillBuildingToProcess()
        {
            await using var sql = new SqlConnection(_connectionString);
            var count = await sql.ExecuteScalarAsync<int>("SELECT Count(*) FROM {Schema.Tools}.ProcessedBuildings");

            if (count > 0)
                return;

            await sql.ExecuteAsync($@"
                INSERT INTO {Schema.Tools}.ProcessedBuildings (BuildingId)
                SELECT DISTINCT b.BuildingPersistentLocalId
                FROM {Schema.BackOffice}.BuildingUnitBuilding");
        }

        // update position OR send message on sqs (=5M tickets)?
        // wait on producer projection if position is more than 100 behind
        // save processed

        // notification to slack to report status

    }
}

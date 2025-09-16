namespace BuildingRegistry.Tools.Console.RepairBuilding
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public sealed class RepairBuildingRepository
    {
        private readonly string _connectionString;

        public RepairBuildingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task EnsureSchemaAndTablesExist()
        {
            await using var sql = new SqlConnection(_connectionString);

            await sql.ExecuteAsync($"CREATE SCHEMA {Schema.Tools}");

            await sql.ExecuteAsync($"CREATE TABLE {Schema.Tools}.ProcessedBuildings (Id INT NOT NULL PRIMARY KEY IDENTITY, BuildingId INT NOT NULL)");
        }

        public async Task<int> GetCount()
        {
            await using var sql = new SqlConnection(_connectionString);
            return await sql.ExecuteScalarAsync<int>($"SELECT Count(*) FROM {Schema.Tools}.ProcessedBuildings");
        }

        public async Task<IEnumerable<int>> GetBuildingsToProcess()
        {
            await using var sql = new SqlConnection(_connectionString);
            return await sql.QueryAsync<int>($"SELECT BuildingId FROM {Schema.Tools}.ProcessedBuildings");
        }

        public async Task FillBuildingToProcess()
        {
            await using var sql = new SqlConnection(_connectionString);
            var count = await GetCount();

            if (count > 0)
                return;

            await sql.ExecuteAsync($@"
                INSERT INTO {Schema.Tools}.ProcessedBuildings (BuildingId)
                SELECT DISTINCT b.BuildingPersistentLocalId
                FROM {Schema.BackOffice}.BuildingUnitBuilding");
        }

        public async Task<int> DeleteBuilding(int buildingPersistentLocalId)
        {
            await using var sql = new SqlConnection(_connectionString);
            return await sql.ExecuteAsync($@"
                DELETE FROM {Schema.Tools}.ProcessedBuildings
                WHERE BuildingId = @BuildingPersistentLocalId", new { BuildingPersistentLocalId = buildingPersistentLocalId });
        }
    }
}

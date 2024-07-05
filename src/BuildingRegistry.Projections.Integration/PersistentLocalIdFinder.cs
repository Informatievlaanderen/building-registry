namespace BuildingRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Npgsql;

    public interface IPersistentLocalIdFinder
    {
        Task<int?> FindBuildingPersistentLocalId(Guid buildingId);
        Task<int?> FindBuildingUnitPersistentLocalId(Guid buildingId, Guid buildingUnitId);
    }

    public class PersistentLocalIdFinder : IPersistentLocalIdFinder
    {
        private readonly string _eventsConnectionString;
        private readonly string _legacyProjectionsConnectionString;
        private readonly string _integrationConnectionString;

        public PersistentLocalIdFinder(
            string eventsConnectionString,
            string legacyProjectionsConnectionString,
            string integrationConnectionString)
        {
            _eventsConnectionString = eventsConnectionString;
            _legacyProjectionsConnectionString = legacyProjectionsConnectionString;
            _integrationConnectionString = integrationConnectionString;
        }

        public async Task<int?> FindBuildingPersistentLocalId(Guid buildingId)
        {
            await using var projectionsConnection = new SqlConnection(_legacyProjectionsConnectionString);

            var sqlProjections = @"
SELECT TOP 1 PersistentLocalId
FROM [building-registry].[BuildingRegistryLegacy].[BuildingSyndicationWithCount]
WHERE BuildingId = @BuildingId AND PersistentLocalId IS NOT NULL";

            var buildingPersistentLocalIdByProjection = await projectionsConnection.QuerySingleOrDefaultAsync<int?>(
                sqlProjections, new { BuildingId = buildingId });

            if(buildingPersistentLocalIdByProjection.HasValue)
                return buildingPersistentLocalIdByProjection;

            await using var connection = new SqlConnection(_eventsConnectionString);

            var sql = @"
SELECT top 1 Json_Value(JsonData, '$.persistentLocalId') AS ""BuildingPersistentLocalId""
FROM [building-registry-events].[BuildingRegistry].[Streams] as s
INNER JOIN [building-registry-events].[BuildingRegistry].[Messages] as m
    on s.IdInternal = m.StreamIdInternal and m.[Type] = 'BuildingPersistentLocalIdentifierWasAssigned'
WHERE s.Id = @BuildingId";

            var buildingPersistentLocalId = await connection.QuerySingleAsync<int>(
                sql, new { BuildingId = buildingId.ToString("D") });

            return buildingPersistentLocalId;
        }

        public async Task<int?> FindBuildingUnitPersistentLocalId(Guid buildingId, Guid buildingUnitId)
        {
            await using var integrationConnection = new NpgsqlConnection(_integrationConnectionString);
            var buildingUnitPersistentLocalIdByIntegration = await integrationConnection.QuerySingleOrDefaultAsync<int?>(
                "SELECT building_unit_persistent_local_id FROM integration_building.building_unit_ids WHERE building_unit_id = @BuildingUnitId",
                new { BuildingUnitId = buildingUnitId });

            if(buildingUnitPersistentLocalIdByIntegration.HasValue)
                return buildingUnitPersistentLocalIdByIntegration;

            await using var projectionsConnection = new SqlConnection(_legacyProjectionsConnectionString);

            var sqlProjections = @"
SELECT TOP 1 bu.PersistentLocalId
FROM [building-registry].[BuildingRegistryLegacy].[BuildingUnitSyndicationWithCount] bu
INNER JOIN [building-registry].[BuildingRegistryLegacy].[BuildingSyndicationWithCount] b
    on b.Position = bu.Position and b.BuildingId = @BuildingId
WHERE bu.BuildingUnitId = @BuildingUnitId AND bu.PersistentLocalId IS NOT NULL";

            var buildingUnitPersistentLocalIdByProjection = await projectionsConnection.QuerySingleOrDefaultAsync<int?>(
                sqlProjections, new { BuildingId = buildingId, BuildingUnitId = buildingUnitId });

            if(buildingUnitPersistentLocalIdByProjection.HasValue)
                return buildingUnitPersistentLocalIdByProjection;

            await using var connection = new SqlConnection(_eventsConnectionString);

            var sql = @"
SELECT top 1 Json_Value(JsonData, '$.persistentLocalId') AS ""BuildingUnitPersistentLocalId""
FROM [building-registry-events].[BuildingRegistry].[Streams] as s
INNER JOIN [building-registry-events].[BuildingRegistry].[Messages] as m
    on s.IdInternal = m.StreamIdInternal and m.[Type] = 'BuildingUnitPersistentLocalIdentifierWasAssigned'
WHERE
    s.Id = @BuildingId
    AND  JSON_VALUE(JsonData, '$.buildingUnitId') = @BuildingUnitId";

            var buildingPersistentLocalId = await connection.QuerySingleAsync<int>(
                sql, new { BuildingId = buildingId.ToString("D"), BuildingUnitId = buildingUnitId });

            return buildingPersistentLocalId;
        }
    }
}

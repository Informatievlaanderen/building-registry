namespace BuildingRegistry.Projections.Integration
{
    using System;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Dapper;
    using Microsoft.Data.SqlClient;

    public interface IPersistentLocalIdFinder
    {
        Task<int?> FindBuildingPersistentLocalId(Guid buildingId);
        Task<int?> FindBuildingUnitPersistentLocalId(Guid buildingId, Guid buildingUnitId);
    }

    public class PersistentLocalIdFinder : IPersistentLocalIdFinder
    {
        private readonly string _eventsConnectionString;

        public PersistentLocalIdFinder(string eventsConnectionString)
        {
            _eventsConnectionString = eventsConnectionString;
        }

        public async Task<int?> FindBuildingPersistentLocalId(Guid buildingId)
        {
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

namespace BuildingRegistry.Console.SortToProcess
{
    using System;
    using System.IO;
    using System.Linq;
    using Api.CrabImport.Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Json;
    using Dapper;
    using Legacy.Commands.Crab;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public static class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var eventsConnectionString = configuration.GetConnectionString("Events");

            var toProcessPath = "ToProcess";
            var rejectsPath = "Rejects";
            var tbdPath = "TBD";

            Directory.CreateDirectory(toProcessPath);
            Directory.CreateDirectory(rejectsPath);
            Directory.CreateDirectory(tbdPath);

            var files = Directory.GetFiles("FilesToProcess");

            var jsonSerializerSettings = new JsonSerializerSettings().ConfigureForCrabImports();

            foreach (var file in files)
            {
                Console.WriteLine($"Processing {file}");
                var subaddressCommands =
                    JsonConvert.DeserializeObject<RegisterCrabImportRequest[]>(File.ReadAllText(file), jsonSerializerSettings)
                        .Select(x => JsonConvert.DeserializeObject<ImportSubaddressFromCrab>(x.CrabItem, jsonSerializerSettings))
                        .ToList();

                int terrainObjectId = subaddressCommands.First().TerrainObjectId;

                var subaddressByTerrainObjectHouseNumbers = subaddressCommands.GroupBy(x => (int)x.TerrainObjectHouseNumberId, x => (int)x.SubaddressId);
                using var sqlConnection = new SqlConnection(eventsConnectionString);
                var hasTbd = false;
                var hasToProcess = false;

                foreach (var subaddressByTerrainObjectHouseNumber in subaddressByTerrainObjectHouseNumbers)
                {
                    var subaddressIds = sqlConnection.Query<int>(@"
                            select JSON_VALUE(jsondata, '$.subaddressId') from [building-registry-events].[BuildingRegistry].[Messages]
                            where StreamIdInternal in (
                                select IdInternal
                                from[building-registry-events].[BuildingRegistry].Streams
                                where id in (
                                    SELECT buildingId FROM[building-registry].[BuildingRegistryLegacy].[BuildingPersistentIdCrabIdMappings] where crabterrainobjectid = @terrainobjectid))
                            and[type] = 'AddressSubaddressWasImportedFromCrab' and JSON_VALUE(jsondata, '$.terrainObjectHouseNumberId') = @terrainobjecthousenumberid", new { terrainobjectid = terrainObjectId, terrainobjecthousenumberid = subaddressByTerrainObjectHouseNumber.Key });

                    if (subaddressByTerrainObjectHouseNumber.All(x => subaddressIds.Contains(x)))
                    {
                        continue;
                    }

                    if (subaddressByTerrainObjectHouseNumber.Any(x => subaddressIds.Contains(x)))
                    {
                        hasTbd = true;
                    }
                    else
                    {
                        hasToProcess = true;
                    }
                }

                if (!hasToProcess && !hasTbd)
                {
                    File.Copy(file, Path.Combine(rejectsPath, Path.GetFileName(file)));
                }
                else if (hasTbd)
                {
                    File.Copy(file, Path.Combine(tbdPath, Path.GetFileName(file)));
                }
                else
                {
                    File.Copy(file, Path.Combine(toProcessPath, Path.GetFileName(file)));
                }
            }

            //Don't forget to post an RequestPersistentId command!
        }
    }
}

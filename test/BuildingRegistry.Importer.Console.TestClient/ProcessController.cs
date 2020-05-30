namespace BuildingRegistry.Importer.Console.TestClient
{
    using System;
    using System.Threading;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Dapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("process")]
    [ApiExplorerSettings(GroupName = "Process")]
    public class ProcessController : ApiController
    {
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult GetBuildings(
            [FromServices] IConfiguration configuration,
            [FromQuery] string id,
            [FromQuery] string from,
            [FromQuery] ImportMode mode = ImportMode.Update,
            CancellationToken cancellationToken = default)
        {
            if (mode == ImportMode.Init)
                return BadRequest("");

            Func<CRABEntities> crabEntitiesFactory = () => new CRABEntities(configuration.GetConnectionString("CRABEntities"));

            var terrainObjectId = new CrabTerrainObjectId(Convert.ToInt32(id));
            var generator = new CommandGenerator(configuration.GetConnectionString("Crab2Vbr"), crabEntitiesFactory, true);
            var settings = new SettingsBasedConfig(configuration.GetSection("ApplicationSettings"));
            var fromDate = DateTime.MinValue;

            if (mode == ImportMode.Update && !DateTime.TryParse(from, out fromDate))
            {
                return BadRequest(string.IsNullOrWhiteSpace(from)
                        ? "Please specify 'from' parameter for an update"
                        : $"Cannot parse parameter 'from' with value '{from}'");
            }

            var commandProcessor = new CommandProcessorBuilder<int>(generator)
                .UseApiProxyFactory(logger =>
                    new TestClientHttpApiProxyFactory(
                        logger,
                        settings,
                        fromDate,
                        terrainObjectId,
                        mode))
                .UseProcessedKeysSet(new NonPersistentProcessedKeysSet<int>())
                .UseCommandProcessorConfig(settings)
                .UseDefaultSerializerSettingsForCrabImports()
                .UseLoggerFactory(new LoggerFactory())
                .Build();

            commandProcessor.Run(null, settings);

            var osloId = GetOsloId(terrainObjectId, configuration.GetConnectionString("Events"));

            return Ok(osloId);
        }

        private static string GetOsloId(CrabTerrainObjectId crabTerrainObjectId, string eventsConnectionString)
        {
            var streamId = crabTerrainObjectId.CreateDeterministicId();
            using (var connection = new SqlConnection(eventsConnectionString))
            {
                return connection.QueryFirstOrDefault<string>(@"
select JSON_VALUE(JsonData, '$.persistentLocalId') as osloid from buildingregistry.Messages m
inner join BuildingRegistry.Streams s on m.StreamIdInternal = s.IdInternal
where s.IdOriginal = @streamId AND [Type] = 'BuildingPersistentLocalIdentifierWasAssigned'",
                    new { streamId });
            }
        }
    }
}

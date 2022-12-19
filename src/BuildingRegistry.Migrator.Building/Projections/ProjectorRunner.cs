namespace BuildingRegistry.Migrator.Building.Projections
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using BuildingRegistry.Infrastructure;
    using Dapper;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using Org.BouncyCastle.Math.EC.Rfc7748;

    public class ProjectorRunner
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly string _connectionString;
        private readonly ILogger _logger;
        public ProjectorRunner(
            IConnectedProjectionsManager projectionsManager,
            ILoggerFactory loggerFactory,
            string connectionString)
        {
            _projectionsManager = projectionsManager;
            _connectionString = connectionString;
            _logger = loggerFactory.CreateLogger<ProjectorRunner>();
        }

        public async Task StartAsync(long startingPosition, CancellationToken cancellationToken = default)
        {
            // Insert ProjectionState position
            await using var conn = new SqlConnection(_connectionString);

            var projectionId = typeof(MigratorProjection).FullName;

            var result = await conn.QueryAsync(
                $@"SELECT *
                FROM [{Schema.MigrateBuilding}].[ProjectionStates]
                WHERE NAME = '{projectionId}'");

            if (result is null || !result.Any())
            {
                await conn.ExecuteAsync($@"INSERT INTO [{Schema.MigrateBuilding}].[ProjectionStates]
                                    ([Name] ,[Position]) VALUES ('{projectionId}' , {startingPosition})");
            }


            _logger.LogInformation("Projector starting");
            await _projectionsManager.Start(cancellationToken);
            _logger.LogInformation("Projector started");

            await Task.Delay(10000, cancellationToken); //waiting for projections to get started

            while (!cancellationToken.IsCancellationRequested
                   && _projectionsManager
                       .GetRegisteredProjections()
                       .All(x => x.State != ConnectedProjectionState.Stopped))
            {
                await Task.Delay(1000, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Projector cancelled");
            }
            else
            {
                _logger.LogCritical("Projections went in a 'stopped' stated");
            }
        }
    }
}

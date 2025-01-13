namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class SnapshotProducers : BackgroundService
    {
        private readonly IConnectedProjectionsManager _projectionsManager;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<SnapshotProducers> _logger;

        public SnapshotProducers(
            IConnectedProjectionsManager projectionsManager,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _projectionsManager = projectionsManager;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = loggerFactory.CreateLogger<SnapshotProducers>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting snapshot projections");
                await _projectionsManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"An error occurred while starting the {nameof(SnapshotProducers)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}

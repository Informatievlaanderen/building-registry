namespace BuildingRegistry.Producer
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class BuildingProducer : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<BuildingProducer> _logger;
        private readonly IConnectedProjectionsManager _projectionManager;

        public BuildingProducer(
            IHostApplicationLifetime hostApplicationLifetime,
            IConnectedProjectionsManager projectionManager,
            ILogger<BuildingProducer> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _projectionManager = projectionManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _projectionManager.Start(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, $"Critical error occured in {nameof(BuildingProducer)}.");
                _hostApplicationLifetime.StopApplication();
                throw;
            }
        }
    }
}

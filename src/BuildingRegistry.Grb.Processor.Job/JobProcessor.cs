namespace BuildingRegistry.Grb.Processor.Job
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class JobProcessor : BackgroundService
    {
        private readonly ILogger<JobProcessor> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;


        public JobProcessor(ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = loggerFactory.CreateLogger<JobProcessor>();
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Monitor Job, start in sequence
            // Process Job Records
            // Process Job Records tickets
            // Create result set
            // Archive Job
            _logger.LogWarning("JobProcessor started");

            _hostApplicationLifetime.StopApplication();

            await Task.FromResult(stoppingToken);
        }
    }
}

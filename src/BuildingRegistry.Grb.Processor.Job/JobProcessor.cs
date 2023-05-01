namespace BuildingRegistry.Grb.Processor.Job
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public sealed class JobProcessor : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Monitor Job, start in sequence
            // Process Job Records
            // Process Job Records tickets
            // Create result set
            // Archive Job
            throw new System.NotImplementedException();
        }
    }
}

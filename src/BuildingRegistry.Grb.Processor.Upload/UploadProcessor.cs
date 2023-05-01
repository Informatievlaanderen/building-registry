namespace BuildingRegistry.Grb.Processor.Upload
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public sealed class UploadProcessor : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check created jobs
            // See if there's a S3 object with job id
            // If so, update ticket status and job status => preparing
            // extract, verify, and store data as job records
            // update job status => prepared
            // trigger job processor
            throw new System.NotImplementedException();
        }
    }
}

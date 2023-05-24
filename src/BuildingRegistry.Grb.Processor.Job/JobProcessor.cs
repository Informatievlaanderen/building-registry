namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using TicketingService.Abstractions;

    public sealed class JobProcessor : BackgroundService
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly IJobRecordsProcessor _jobRecordsProcessor;
        private readonly IJobRecordsMonitor _jobRecordsMonitor;
        private readonly ITicketing _ticketing;
        private readonly IJobResultUploader _jobResultUploader;
        private readonly IJobRecordsArchiver _jobRecordsArchiver;
        private readonly GrbApiOptions _grbApiOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<JobProcessor> _logger;

        public JobProcessor(
            BuildingGrbContext buildingGrbContext,
            IJobRecordsProcessor jobRecordsProcessor,
            IJobRecordsMonitor jobRecordsMonitor,
            IJobResultUploader jobResultUploader,
            IJobRecordsArchiver jobRecordsArchiver,
            ITicketing ticketing,
            IOptions<GrbApiOptions> grbApiOptions,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _buildingGrbContext = buildingGrbContext;
            _jobRecordsProcessor = jobRecordsProcessor;
            _jobRecordsMonitor = jobRecordsMonitor;
            _ticketing = ticketing;
            _jobResultUploader = jobResultUploader;
            _jobRecordsArchiver = jobRecordsArchiver;
            _grbApiOptions = grbApiOptions.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = loggerFactory.CreateLogger<JobProcessor>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int maxLifeTimeJob = 65;

            _logger.LogInformation("JobProcessor started");

            var inactiveJobStatuses = new[] {JobStatus.Completed, JobStatus.Cancelled};
            var jobsToProcess = await _buildingGrbContext.Jobs
                .Where(x => !inactiveJobStatuses.Contains(x.Status))
                .OrderBy(x => x.Created)
                .ToListAsync(stoppingToken);

            foreach (var job in jobsToProcess)
            {
                if (job.Status == JobStatus.Created)
                {
                    if (job.IsExpired(TimeSpan.FromMinutes(maxLifeTimeJob)))
                    {
                        await CancelJob(job, stoppingToken);
                        continue;
                    }
                    break;
                }

                if (job.Status is JobStatus.Preparing or JobStatus.Error)
                {
                    _logger.LogWarning("Job '{jobId}' cannot be processed because it has status '{jobStatus}'.", job.Id, job.Status);
                    break;
                }

                await ProcessJob(job, stoppingToken);
            }

            _hostApplicationLifetime.StopApplication();

            await Task.FromResult(stoppingToken);
        }

        private async Task ProcessJob(Job job, CancellationToken stoppingToken)
        {
            await UpdateJobStatus(job, JobStatus.Processing, stoppingToken);

            await _jobRecordsProcessor.Process(job.Id, stoppingToken);
            await _jobRecordsMonitor.Monitor(job.Id, stoppingToken);

            var jobRecordErrors = await _buildingGrbContext.JobRecords
                .Where(x =>
                    x.JobId == job.Id
                    && x.Status == JobRecordStatus.Error)
                .ToListAsync(stoppingToken);

            if (jobRecordErrors.Any())
            {
                var jobErrors = jobRecordErrors.Select(x => new TicketError(x.ErrorMessage!, string.Empty)).ToList();
                await _ticketing.Error(job.TicketId!.Value, new TicketError(jobErrors), stoppingToken);

                await UpdateJobStatus(job, JobStatus.Error, stoppingToken);

                return;
            }

            await _jobResultUploader.UploadJobResults(job.Id, stoppingToken);

            await _ticketing.Complete(
                job.TicketId!.Value,
                new TicketResult(new
                {
                    JobResultLocation = new Uri(new Uri(_grbApiOptions.GrbApiUrl), $"/uploads/jobs/{job.Id:D}/results").ToString()
                }),
                stoppingToken);
            await UpdateJobStatus(job, JobStatus.Completed, stoppingToken);

            await _jobRecordsArchiver.Archive(job.Id, stoppingToken);
        }

        private async Task CancelJob(Job job, CancellationToken stoppingToken)
        {
            await UpdateJobStatus(job, JobStatus.Cancelled, stoppingToken);
            _logger.LogWarning("Cancelled expired job '{jobId}'.", job.Id);
        }

        private async Task UpdateJobStatus(Job job, JobStatus jobStatus, CancellationToken stoppingToken)
        {
            job.UpdateStatus(jobStatus);
            await _buildingGrbContext.SaveChangesAsync(stoppingToken);
        }
    }
}

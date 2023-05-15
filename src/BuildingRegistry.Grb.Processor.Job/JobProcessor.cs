﻿namespace BuildingRegistry.Grb.Processor.Job
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
        private readonly GrbApiOptions _grbApiOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<JobProcessor> _logger;

        public JobProcessor(
            BuildingGrbContext buildingGrbContext,
            IJobRecordsProcessor jobRecordsProcessor,
            IJobRecordsMonitor jobRecordsMonitor,
            ITicketing ticketing,
            IOptions<GrbApiOptions> grbApiOptions,
            IHostApplicationLifetime hostApplicationLifetime,
            ILoggerFactory loggerFactory)
        {
            _buildingGrbContext = buildingGrbContext;
            _jobRecordsProcessor = jobRecordsProcessor;
            _jobRecordsMonitor = jobRecordsMonitor;
            _ticketing = ticketing;
            _grbApiOptions = grbApiOptions.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = loggerFactory.CreateLogger<JobProcessor>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const int maxLifeTimeJob = 65;

            // Monitor Job, start in sequence
            // Process Job Records
            // Monitor Job Records tickets
            // Create result set
            // Archive Job
            _logger.LogInformation("JobProcessor started");

            // Check for jobs with status prepared
            var inactiveJobStatuses = new JobStatus[] { JobStatus.Completed, JobStatus.Cancelled };
            var jobsToProcess = _buildingGrbContext.Jobs
                .Where(x => !inactiveJobStatuses.Contains(x.Status))
                .OrderBy(x => x.Created);

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

                job.UpdateStatus(JobStatus.Processing);
                await _buildingGrbContext.SaveChangesAsync(stoppingToken);

                var jobRecords = await _buildingGrbContext.JobRecords
                    .Where(x => x.JobId == job.Id)
                    .ToListAsync(stoppingToken);

                await _jobRecordsProcessor.Process(jobRecords, stoppingToken);
                await _jobRecordsMonitor.Monitor(jobRecords, stoppingToken);

                if (jobRecords.Any(x => x.Status == JobRecordStatus.Error))
                {
                    job.UpdateStatus(JobStatus.Error);
                    await _buildingGrbContext.SaveChangesAsync(stoppingToken);
                }
                else
                {
                    foreach (var jobRecord in jobRecords.Where(x => x.Status is JobRecordStatus.Complete or JobRecordStatus.Warning))
                    {
                        var jobResult = new JobResult
                        {
                            // BuildingPersistentLocalId = jobRecord.BuildingPersistentLocalId // Can be null when warning. E.g. gebouw is verwijderd
                            GrbIdn = (int)jobRecord.Idn,
                            IsBuildingCreated = jobRecord.EventType == GrbEventType.DefineBuilding,
                            JobId = jobRecord.JobId
                        };
                        _buildingGrbContext.JobResults.Add(jobResult);
                    }

                    job.UpdateStatus(JobStatus.Completed);
                    await _buildingGrbContext.SaveChangesAsync(stoppingToken);

                    await _ticketing.Complete(
                        job.TicketId!.Value,
                        new TicketResult(new
                        {
                            JobResultLocation = new Uri(new Uri(_grbApiOptions.GrbApiUrl), $"/uploads/jobs/{job.Id}").ToString()
                        }),
                        stoppingToken);
                }
            }

            _hostApplicationLifetime.StopApplication();

            await Task.FromResult(stoppingToken);
        }

        private async Task CancelJob(Job job, CancellationToken stoppingToken)
        {
            job.UpdateStatus(JobStatus.Cancelled);
            await _buildingGrbContext.SaveChangesAsync(stoppingToken);
            _logger.LogWarning("Cancelled expired job '{jobId}'.", job.Id);
        }
    }
}

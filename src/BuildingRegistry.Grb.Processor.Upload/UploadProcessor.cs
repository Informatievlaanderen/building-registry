namespace BuildingRegistry.Grb.Processor.Upload
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Amazon.ECS;
    using Amazon.ECS.Model;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using TicketingService.Abstractions;
    using Zip;
    using Zip.Translators;
    using Zip.Validators;
    using Task = System.Threading.Tasks.Task;

    public sealed class UploadProcessor : BackgroundService
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly ITicketing _ticketing;
        private readonly IBlobClient _blobClient;
        private readonly IAmazonECS _amazonEcs;
        private readonly ILogger<UploadProcessor> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly EcsTaskOptions _ecsTaskOptions;

        public UploadProcessor(
            BuildingGrbContext buildingGrbContext,
            ITicketing ticketing,
            IBlobClient blobClient,
            IAmazonECS amazonEcs,
            ILoggerFactory loggerFactory,
            IHostApplicationLifetime hostApplicationLifetime,
            IOptions<EcsTaskOptions> ecsTaskOptions)
        {
            _buildingGrbContext = buildingGrbContext;
            _ticketing = ticketing;
            _blobClient = blobClient;
            _amazonEcs = amazonEcs;
            _logger = loggerFactory.CreateLogger<UploadProcessor>();
            _hostApplicationLifetime = hostApplicationLifetime;
            _ecsTaskOptions = ecsTaskOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check created jobs
            var jobs = await _buildingGrbContext.Jobs
                .Where(x => x.Status == JobStatus.Created || x.Status == JobStatus.Preparing)
                .OrderBy(x => x.Created)
                .ToListAsync(stoppingToken);

            if (!jobs.Any())
            {
                _hostApplicationLifetime.StopApplication();
                return;
            }

            foreach (var job in jobs)
            {
                try
                {
                    await using var stream = await GetZipArchiveStream(job, stoppingToken);

                    if (stream == null)
                    {
                        continue;
                    }

                    // If so, update ticket status and job status => preparing
                    await _ticketing.Pending(job.TicketId!.Value, stoppingToken);
                    await UpdateJobStatus(job, JobStatus.Preparing, stoppingToken);

                    using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

                    var archiveValidator = new ZipArchiveValidator(GrbArchiveEntryStructure);

                    var problems = archiveValidator.Validate(archive);
                    if (problems.Any())
                    {
                        var ticketingErrors = problems.Select(x =>
                                x.Parameters.Any()
                                    ? new TicketError(string.Join(',', x.Parameters.Select(y => y.Value)), x.Reason)
                                    : new TicketError(x.File, x.Reason))
                            .ToList();

                        await _ticketing.Error(job.TicketId!.Value, new TicketError(ticketingErrors), stoppingToken);
                        await UpdateJobStatus(job, JobStatus.Error, stoppingToken);

                        continue;
                    }

                    var archiveTranslator = new ZipArchiveTranslator(Encoding.UTF8);
                    var jobRecords = archiveTranslator.Translate(archive).ToList();
                    jobRecords.ForEach(x => x.JobId = job.Id);

                    await _buildingGrbContext.JobRecords.AddRangeAsync(jobRecords, stoppingToken);
                    await _buildingGrbContext.SaveChangesAsync(stoppingToken);

                    await UpdateJobStatus(job, JobStatus.Prepared, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unexpected exception for job '{job.Id}'", ex);
                    await _ticketing.Error(job.TicketId!.Value, new TicketError($"Onverwachte fout bij de verwerking van het zip-bestand.", string.Empty), stoppingToken);
                    await UpdateJobStatus(job, JobStatus.Error, stoppingToken);
                }
            }

            if (jobs.Any(x => x.Status == JobStatus.Prepared))
            {
                await StartJobProcessor(stoppingToken);
            }

            _hostApplicationLifetime.StopApplication();
        }

        private async Task UpdateJobStatus(Job job, JobStatus status, CancellationToken stoppingToken)
        {
            job.UpdateStatus(status);
            await _buildingGrbContext.SaveChangesAsync(stoppingToken);
        }

        private async Task StartJobProcessor(CancellationToken stoppingToken)
        {
            var taskResponse = await _amazonEcs.StartTaskAsync(new StartTaskRequest()
            {
                TaskDefinition = _ecsTaskOptions.TaskDefinition,
                Cluster = _ecsTaskOptions.Cluster
            }, stoppingToken);

            if (taskResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"Starting ECS Task return HttpStatusCode:{taskResponse.HttpStatusCode.ToString()}");
            }

            string FailureToString(Failure failure) =>
                $"Reason: {failure.Reason}{Environment.NewLine}Failure:{failure.Detail}";

            if (taskResponse.Failures.Any())
            {
                foreach (var failure in taskResponse.Failures)
                {
                    _logger.LogError(FailureToString(failure));
                }
            }
        }

        private async Task<Stream?> GetZipArchiveStream(Job job, CancellationToken stoppingToken)
        {
            var blobName = new BlobName(job.ReceivedBlobName);

            if (!await _blobClient.BlobExistsAsync(blobName, stoppingToken))
            {
                return null;
            }

            var blobObject = await _blobClient.GetBlobAsync(blobName, stoppingToken);
            if (blobObject is null)
            {
                _logger.LogError($"No blob found with name: {job.ReceivedBlobName}");
                return null;
            }

            try
            {
                return await blobObject.OpenAsync(stoppingToken);
            }
            catch (BlobNotFoundException)
            {
                _logger.LogError($"No blob found with name: {job.ReceivedBlobName}");
                return null;
            }
        }

        public static Dictionary<string, IZipArchiveEntryValidator> GrbArchiveEntryStructure =>
            new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    ZipArchiveConstants.DBF_FILENAME,
                    new ZipArchiveDbaseEntryValidator<GrbDbaseRecord>(
                        Encoding.UTF8,
                        new DbaseFileHeaderReadBehavior(true),
                        new GrbDbaseSchema(),
                        new GrbDbaseRecordsValidator())
                },
                {
                    ZipArchiveConstants.SHP_FILENAME,
                    new ZipArchiveShapeEntryValidator(Encoding.UTF8, new GrbShapeRecordsValidator())
                }
            };
    }
}

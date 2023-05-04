namespace BuildingRegistry.Grb.Processor.Upload
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;
    using Zip;
    using Zip.Translators;
    using Zip.Validators;

    public sealed class UploadProcessor : BackgroundService
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly ITicketing _ticketing;
        private readonly IBlobClient _blobClient;
        private readonly ILogger<UploadProcessor> _logger;

        public UploadProcessor(
            BuildingGrbContext buildingGrbContext,
            ITicketing ticketing,
            IBlobClient blobClient,
            ILoggerFactory loggerFactory)
        {
            _buildingGrbContext = buildingGrbContext;
            _ticketing = ticketing;
            _blobClient = blobClient;
            _logger = loggerFactory.CreateLogger<UploadProcessor>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Check created jobs
            var createdJobs = await _buildingGrbContext.Jobs
                .Where(x => x.Status == JobStatus.Created || x.Status == JobStatus.Preparing)
                .OrderBy(x => x.Created)
                .ToListAsync(stoppingToken);

            foreach (var job in createdJobs)
            {
                var jobName = new BlobName("received/"+job.BlobName);

                // See if there's a S3 object with job id
                if (!await _blobClient.BlobExistsAsync(jobName, stoppingToken))
                {
                    continue;
                }

                // If so, update ticket status and job status => preparing
                await _ticketing.Pending(job.TicketId!.Value, stoppingToken);
                job.Status = JobStatus.Preparing;
                await _buildingGrbContext.SaveChangesAsync(stoppingToken);

                var blobObject = await _blobClient.GetBlobAsync(jobName, stoppingToken);
                if (blobObject is null)
                {
                    _logger.LogError($"No blob found with name: {job.BlobName}");
                    continue;
                }

                try
                {
                    // extract, verify, and store data as job records
                    await using var stream = await blobObject.OpenAsync(stoppingToken);
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

                        job.Status = JobStatus.Error;
                        await _buildingGrbContext.SaveChangesAsync(stoppingToken);

                        continue;
                    }

                    var archiveTranslator = new ZipArchiveTranslator(Encoding.UTF8);
                    var jobRecords = archiveTranslator.Translate(archive);

                    foreach (var jobRecord in jobRecords)
                    {
                        jobRecord.JobId = job.Id;
                    }
                    await _buildingGrbContext.JobRecords.AddRangeAsync(jobRecords, stoppingToken);
                    await _buildingGrbContext.SaveChangesAsync(stoppingToken);
                }
                catch (BlobNotFoundException)
                {
                    _logger.LogError($"No blob found with name: {job.BlobName}");
                    continue;
                }

                job.Status = JobStatus.Prepared;
                await _buildingGrbContext.SaveChangesAsync(stoppingToken);
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

namespace BuildingRegistry.Grb.Processor.Upload
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Zip.Translators;

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
                .Where(x => x.Status == JobStatus.Created)
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

                    var archiveTranslator = new ZipArchiveTranslator(Encoding.GetEncoding(1252));
                    archiveTranslator.Translate(archive);
                }
                catch (BlobNotFoundException)
                {
                    _logger.LogError($"No blob found with name: {job.BlobName}");
                    continue;
                }

                job.Status = JobStatus.Prepared;
                await _buildingGrbContext.SaveChangesAsync(stoppingToken);
            }


            // extract, verify, and store data as job records
            // update job status => prepared
            // trigger job processor
            throw new System.NotImplementedException();
        }
    }
}

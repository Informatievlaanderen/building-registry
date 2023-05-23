namespace BuildingRegistry.Grb.Processor.Job
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.EntityFrameworkCore;

    public interface IJobResultUploader
    {
        Task UploadJobResults(Guid jobId, CancellationToken ct);
    }

    public class JobResultUploader : IJobResultUploader
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly IBlobClient _blobClient;

        public JobResultUploader(BuildingGrbContext buildingGrbContext, IBlobClient blobClient)
        {
            _buildingGrbContext = buildingGrbContext;
            _blobClient = blobClient;
        }

        public async Task UploadJobResults(Guid jobId, CancellationToken ct)
        {
            var jobResultsZipArchive = await CreateResultFile(jobId, ct);

            using var stream = new MemoryStream();
            jobResultsZipArchive.WriteTo(stream, ct);

            var metadata = Metadata.None.Add(
                new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), jobResultsZipArchive.Name));

            await _blobClient.CreateBlobAsync(
                new BlobName(Job.JobResultsBlobName(jobId)),
                metadata,
                ContentType.Parse("application/zip"),
                stream,
                ct);
        }

        private async Task<ExtractFile> CreateResultFile(Guid jobId, CancellationToken ct)
        {
            var jobResults = await GetJobResults(jobId, ct);

            byte[] TransformRecord(JobResult jobResult)
            {
                var item = new JobResultDbaseRecord
                {
                    idn = {Value = jobResult.GrbIdn},
                    grid = {Value = jobResult.BuildingPersistentLocalId},
                };

                return item.ToBytes(DbfFileWriter<JobResultDbaseRecord>.Encoding);
            }

            return ExtractBuilder.CreateDbfFile<JobResult, JobResultDbaseRecord>(
                "IdnGrResults",
                new JobResultDbaseSchema(),
                jobResults,
                jobResults.Count,
                TransformRecord);
        }

        private async Task<IEnumerable<JobResult>> GetJobResults(Guid jobId, CancellationToken ct)
        {
            var jobRecords = await _buildingGrbContext.JobRecords
                .AsNoTracking()
                .Where(x =>
                    x.JobId == jobId
                    && (x.Status == JobRecordStatus.Complete || x.Status == JobRecordStatus.Warning)
                    && x.EventType == GrbEventType.DefineBuilding)
                .ToListAsync(ct);

            return jobRecords
                .Select(jobRecord =>
                    new JobResult
                    {
                        JobId = jobRecord.JobId,
                        BuildingPersistentLocalId = jobRecord.BuildingPersistentLocalId ?? jobRecord.GrId,
                        GrbIdn = (int)jobRecord.Idn,
                        IsBuildingCreated = jobRecord.EventType == GrbEventType.DefineBuilding
                    }).ToList();
        }
    }
}

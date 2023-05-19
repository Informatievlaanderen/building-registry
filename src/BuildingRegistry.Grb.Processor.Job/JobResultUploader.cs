namespace BuildingRegistry.Grb.Processor.Job
{
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
        Task UploadJobResults(IEnumerable<JobRecord> jobRecords, CancellationToken ct);
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

        public async Task UploadJobResults(IEnumerable<JobRecord> jobRecords, CancellationToken ct)
        {
            var jobResultsZipArchive = await CreateResultFile(jobRecords, ct);

            var stream = new MemoryStream();
            jobResultsZipArchive.WriteTo(stream, ct);

            var metadata = Metadata.None.Add(
                new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), jobResultsZipArchive.Name));

            await _blobClient.CreateBlobAsync(
                new BlobName(Job.JobResultsBlobName(jobRecords.First().JobId)),
                metadata,
                ContentType.Parse("application/zip"),
                stream,
                ct);
        }

        private async Task<ExtractFile> CreateResultFile(IEnumerable<JobRecord> jobRecords, CancellationToken ct)
        {
            var jobResults = await GetJobResults(jobRecords, ct);

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

        private async Task<IEnumerable<JobResult>> GetJobResults(IEnumerable<JobRecord> jobRecords, CancellationToken ct)
        {
            // return _buildingGrbContext
            //     .JobResults
            //     .AsNoTracking()
            //     .Where(result => result.JobId == jobId && result.IsBuildingCreated);

            var filteredJobRecords = await _buildingGrbContext.JobRecords
                .Where(x =>
                    (x.Status == JobRecordStatus.Complete || x.Status == JobRecordStatus.Warning)
                    && x.EventType == GrbEventType.DefineBuilding)
                .ToListAsync(ct);

            return filteredJobRecords
                .Where(x => x.Status is JobRecordStatus.Complete or JobRecordStatus.Warning)
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

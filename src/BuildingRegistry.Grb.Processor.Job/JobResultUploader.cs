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
        Task UploadJob(Job job, CancellationToken ct);
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

        public async Task UploadJob(Job job, CancellationToken ct)
        {
            var jobResultsZipArchive = CreateResultFile(job.Id);

            var stream = new MemoryStream();
            jobResultsZipArchive.WriteTo(stream, ct);

            var metadata = Metadata.None.Add(
                new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"),
                    jobResultsZipArchive.Name));

            await _blobClient.CreateBlobAsync(
                new BlobName($"jobresults/{job.BlobName}"),
                metadata,
                ContentType.Parse("application/zip"),
                stream,
                ct);
        }

        private ExtractFile CreateResultFile(Guid jobId)
        {
            var resultItems = _buildingGrbContext
                .JobResults
                .AsNoTracking()
                .Where(result => result.JobId == jobId && result.IsBuildingCreated);

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
                resultItems,
                resultItems.Count,
                TransformRecord);
        }
    }
}

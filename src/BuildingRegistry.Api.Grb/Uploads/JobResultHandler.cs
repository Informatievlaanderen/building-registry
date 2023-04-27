namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using BuildingRegistry.Grb.Abstractions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public sealed record GetJobResultRequest(Guid JobId) : IRequest<ExtractArchive>;

    public sealed class JobResultHandler : IRequestHandler<GetJobResultRequest, ExtractArchive>
    {
        private readonly BuildingGrbContext _buildingGrbContext;

        public JobResultHandler(BuildingGrbContext buildingGrbContext)
        {
            _buildingGrbContext = buildingGrbContext;
        }

        public async Task<ExtractArchive> Handle(GetJobResultRequest request, CancellationToken cancellationToken)
        {
            var job = await _buildingGrbContext.Jobs.FindAsync(new object[] { request.JobId }, cancellationToken);

            if (job is not { Status: JobStatus.Completed })
            {
                throw new ApiException($"Upload job with id {request.JobId} not found or not completed.", StatusCodes.Status404NotFound);
            }

            return new ExtractArchive($"JobResults_{job.Id:D}.zip")
                {
                    CreateResultFile(job.Id)
                };
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
                    idn = { Value = jobResult.GrbIdn },
                    grid = { Value = jobResult.BuildingPersistentLocalId },
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

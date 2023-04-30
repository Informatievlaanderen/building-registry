namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Grb.Abstractions;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public sealed record CancelJobRequest(Guid JobId) : IRequest;

    public sealed class CancelJobHandler : IRequestHandler<CancelJobRequest>
    {
        private readonly BuildingGrbContext _buildingGrbContext;

        public CancelJobHandler(BuildingGrbContext buildingGrbContext)
        {
            _buildingGrbContext = buildingGrbContext;
        }

        public async Task Handle(CancelJobRequest request, CancellationToken cancellationToken)
        {
            var job = await _buildingGrbContext.Jobs.FindAsync(new object[] { request.JobId }, cancellationToken);

            if (job is null)
                throw new ApiException($"Upload job with id {request.JobId} not found.", StatusCodes.Status404NotFound);
            if(job.Status != JobStatus.Created)
                throw new ApiException($"Upload job with id {request.JobId} is being processed and cannot be cancelled.", StatusCodes.Status400BadRequest);

            job.Status = JobStatus.Cancelled;
            await _buildingGrbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

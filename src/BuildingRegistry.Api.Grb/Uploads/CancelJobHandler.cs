namespace BuildingRegistry.Api.Grb.Uploads
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Grb.Abstractions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using TicketingService.Abstractions;

    public sealed record CancelJobRequest(Guid JobId) : IRequest;

    public sealed class CancelJobHandler : IRequestHandler<CancelJobRequest>
    {
        private readonly BuildingGrbContext _buildingGrbContext;
        private readonly ITicketing _ticketing;

        public CancelJobHandler(
            BuildingGrbContext buildingGrbContext,
            ITicketing ticketing)
        {
            _buildingGrbContext = buildingGrbContext;
            _ticketing = ticketing;
        }

        public async Task Handle(CancelJobRequest request, CancellationToken cancellationToken)
        {
            var job = await _buildingGrbContext.Jobs.FindAsync(new object[] { request.JobId }, cancellationToken);

            if (job is null)
            {
                throw new ApiException($"Upload job with id {request.JobId} not found.", StatusCodes.Status404NotFound);
            }
            if (job.Status != JobStatus.Created)
            {
                throw new ApiException(
                    $"Upload job with id {request.JobId} is being processed and cannot be cancelled.",
                    StatusCodes.Status400BadRequest);
            }

            await _ticketing.Complete(
                job.TicketId!.Value,
                new TicketResult(new { JobStatus = "Cancelled" }),
                cancellationToken);

            job.UpdateStatus(JobStatus.Cancelled);
            await _buildingGrbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

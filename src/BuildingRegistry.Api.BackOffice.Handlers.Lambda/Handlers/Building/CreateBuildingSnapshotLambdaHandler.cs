namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class CreateBuildingSnapshotLambdaHandler : BuildingLambdaHandler<CreateBuildingSnapshotLambdaRequest>
    {
        public CreateBuildingSnapshotLambdaHandler(
             IConfiguration configuration,
             ICustomRetryPolicy retryPolicy,
             ITicketing ticketing,
             IIdempotentCommandHandler idempotentCommandHandler,
             IBuildings buildings)
             : base(
                 configuration,
                 retryPolicy,
                 ticketing,
                 idempotentCommandHandler,
                 buildings)
        { }

        protected override async Task<object> InnerHandle(CreateBuildingSnapshotLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            return "snapshot created";
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CreateBuildingSnapshotLambdaRequest request)
        {
            return null;
        }
    }
}

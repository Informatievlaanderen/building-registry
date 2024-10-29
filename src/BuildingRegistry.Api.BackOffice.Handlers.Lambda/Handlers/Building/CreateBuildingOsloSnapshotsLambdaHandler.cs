namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class CreateBuildingOsloSnapshotsLambdaHandler : SqsLambdaHandlerBase<CreateBuildingOsloSnapshotsLambdaRequest>
    {
        public CreateBuildingOsloSnapshotsLambdaHandler(
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
        }

        protected override async Task<object> InnerHandle(CreateBuildingOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata!,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            return "done";
        }

        protected override TicketError? MapDomainException(DomainException exception, CreateBuildingOsloSnapshotsLambdaRequest request) => null;

        protected override Task HandleAggregateIdIsNotFoundException(CreateBuildingOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateIfMatchHeaderValue(CreateBuildingOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

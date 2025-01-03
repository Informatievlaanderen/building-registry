namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class CreateBuildingUnitOsloSnapshotsLambdaHandler : SqsLambdaHandlerBase<CreateBuildingUnitOsloSnapshotsLambdaRequest>
    {
        public CreateBuildingUnitOsloSnapshotsLambdaHandler(
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
        }

        protected override async Task<object> InnerHandle(CreateBuildingUnitOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? MapDomainException(DomainException exception, CreateBuildingUnitOsloSnapshotsLambdaRequest request) => null;

        protected override Task HandleAggregateIdIsNotFoundException(CreateBuildingUnitOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task ValidateIfMatchHeaderValue(CreateBuildingUnitOsloSnapshotsLambdaRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

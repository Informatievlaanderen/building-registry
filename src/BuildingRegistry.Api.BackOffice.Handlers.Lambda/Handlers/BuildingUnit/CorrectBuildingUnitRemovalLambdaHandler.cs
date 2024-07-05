namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
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
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class CorrectBuildingUnitRemovalLambdaHandler : BuildingUnitLambdaHandler<CorrectBuildingUnitRemovalLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public CorrectBuildingUnitRemovalLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            BackOfficeContext backOfficeContext)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<object> InnerHandle(CorrectBuildingUnitRemovalLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            // Transaction because a commonBuildingUnit is sometimes added
            await using var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken);

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

            await transaction.CommitAsync(cancellationToken);

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectBuildingUnitRemovalLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectBuildingUnitRemoval.InvalidBuildingStatus.Message,
                    ValidationErrors.CorrectBuildingUnitRemoval.InvalidBuildingStatus.Code),

                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectBuildingUnitRemoval.InvalidBuildingUnitStatus.Message,
                    ValidationErrors.CorrectBuildingUnitRemoval.InvalidBuildingUnitStatus.Code),

                BuildingUnitHasInvalidFunctionException => new TicketError(
                    ValidationErrors.Common.CommonBuildingUnit.InvalidFunction.Message,
                    ValidationErrors.Common.CommonBuildingUnit.InvalidFunction.Code),
                _ => null
            };
        }
    }
}

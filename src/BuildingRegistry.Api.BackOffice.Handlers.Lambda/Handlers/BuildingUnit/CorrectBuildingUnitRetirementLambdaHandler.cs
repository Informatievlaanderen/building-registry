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

    public sealed class CorrectBuildingUnitRetirementLambdaHandler : BuildingUnitLambdaHandler<CorrectBuildingUnitRetirementLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public CorrectBuildingUnitRetirementLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectBuildingUnitRetirementLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            // Transaction because a commonBuildingUnit is sometimes added
            await using var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken);

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

            await transaction.CommitAsync(cancellationToken);

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectBuildingUnitRetirementLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectBuildingUnitRetirement.BuildingInvalidStatus.Message,
                    ValidationErrors.CorrectBuildingUnitRetirement.BuildingInvalidStatus.Code),

                BuildingUnitHasInvalidFunctionException => new TicketError(
                    ValidationErrors.Common.CommonBuildingUnit.InvalidFunction.Message,
                    ValidationErrors.Common.CommonBuildingUnit.InvalidFunction.Code),

                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectBuildingUnitRetirement.InvalidStatus.Message,
                    ValidationErrors.CorrectBuildingUnitRetirement.InvalidStatus.Code),
                _ => null
            };
        }
    }
}

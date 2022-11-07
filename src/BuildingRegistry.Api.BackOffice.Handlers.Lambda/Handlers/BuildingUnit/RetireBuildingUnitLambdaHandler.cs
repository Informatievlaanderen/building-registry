namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class RetireBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<RetireBuildingUnitLambdaRequest>
    {
        public RetireBuildingUnitLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(RetireBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
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

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RetireBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrors.RetireBuildingUnit.BuildingInvalidStatus.Message,
                    ValidationErrors.RetireBuildingUnit.BuildingInvalidStatus.Code),
                BuildingUnitHasInvalidFunctionException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitHasInvalidFunction,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitHasInvalidFunction),
                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeRetired,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeRetired),
                _ => null
            };
        }
    }
}

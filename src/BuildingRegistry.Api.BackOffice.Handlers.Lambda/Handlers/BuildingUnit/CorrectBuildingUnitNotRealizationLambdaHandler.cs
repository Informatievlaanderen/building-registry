namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using Abstractions.Exceptions;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class CorrectBuildingUnitNotRealizationLambdaHandler : BuildingUnitLambdaHandler<CorrectBuildingUnitNotRealizationLambdaRequest>
    {
        public CorrectBuildingUnitNotRealizationLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(CorrectBuildingUnitNotRealizationLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? MapDomainException(DomainException exception, CorrectBuildingUnitNotRealizationLambdaRequest request)
        {
            return exception switch
            {
                BuildingUnitHasInvalidFunctionException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitHasInvalidFunction,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitHasInvalidFunction),
                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeCorrectedFromNotRealizedToPlanned,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeCorrectedFromNotRealizedToPlanned),
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeCorrectedFromNotRealizedToPlannedBecauseOfInvalidBuildingStatus,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeCorrectedFromNotRealizedToPlannedBecauseOfInvalidBuildingStatus),
                _ => null
            };
        }
    }
}

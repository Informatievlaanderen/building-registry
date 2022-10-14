namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
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
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class CorrectBuildingRealizationLambdaHandler : BuildingLambdaHandler<CorrectBuildingRealizationLambdaRequest>
    {
        public CorrectBuildingRealizationLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(CorrectBuildingRealizationLambdaRequest request, CancellationToken cancellationToken)
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

            var lastHash = await GetHash(new BuildingPersistentLocalId(request.BuildingPersistentLocalId), cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, CorrectBuildingRealizationLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Building.BuildingCannotBeCorrectedFromRealizedToUnderConstruction,
                    ValidationErrorCodes.Building.BuildingCannotBeCorrectedFromRealizedToUnderConstruction),
                BuildingHasInvalidBuildingGeometryMethodException => new TicketError(
                    ValidationErrorMessages.Building.BuildingIsMeasuredByGrb,
                    ValidationErrorCodes.Building.BuildingIsMeasuredByGrb),
                BuildingHasRetiredBuildingUnitsException => new TicketError(
                    ValidationErrorMessages.Building.BuildingHasRetiredBuildingUnits,
                    ValidationErrorCodes.Building.BuildingHasRetiredBuildingUnits),
                _ => null
            };
        }
    }
}

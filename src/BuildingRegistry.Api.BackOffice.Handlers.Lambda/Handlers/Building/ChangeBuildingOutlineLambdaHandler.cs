namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
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
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class ChangeBuildingOutlineLambdaHandler : BuildingLambdaHandler<ChangeBuildingOutlineLambdaRequest>
    {
        public ChangeBuildingOutlineLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(ChangeBuildingOutlineLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, ChangeBuildingOutlineLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.ChangeBuildingOutline.BuildingInvalidStatus.ToTicketError(),
                BuildingHasInvalidBuildingGeometryMethodException => new TicketError(
                    ValidationErrorMessages.Building.BuildingIsMeasuredByGrb,
                    ValidationErrorCodes.Building.BuildingIsMeasuredByGrb),
                BuildingHasBuildingUnitsOutsideBuildingGeometryException => new TicketError(
                    ValidationErrorMessages.Building.BuildingHasBuildingUnitsOutsideChangedGeometry,
                    ValidationErrorCodes.Building.BuildingHasBuildingUnitsOutsideChangedGeometry),
                _ => null
            };
        }
    }
}

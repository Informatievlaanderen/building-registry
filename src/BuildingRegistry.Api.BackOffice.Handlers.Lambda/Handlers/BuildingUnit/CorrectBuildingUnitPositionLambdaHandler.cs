namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
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

    public sealed class CorrectBuildingUnitPositionLambdaHandler : BuildingUnitLambdaHandler<CorrectBuildingUnitPositionLambdaRequest>
    {
        public CorrectBuildingUnitPositionLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectBuildingUnitPositionLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectBuildingUnitPositionLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.CorrectBuildingUnitPosition.BuildingInvalidStatus.ToTicketError(),
                BuildingUnitHasInvalidFunctionException => ValidationErrors.Common.BuildingUnitHasInvalidFunction.ToTicketError(),
                BuildingUnitHasInvalidStatusException => ValidationErrors.CorrectBuildingUnitPosition.BuildingUnitInvalidStatus.ToTicketError(),
                BuildingUnitPositionIsOutsideBuildingGeometryException => ValidationErrors.CorrectBuildingUnitPosition.BuildingUnitPositionOutsideBuildingGeometry.ToTicketError(),
                _ => null
            };
        }
    }
}

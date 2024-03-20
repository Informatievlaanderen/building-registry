namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
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

        protected override async Task<object> InnerHandle(CorrectBuildingRealizationLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectBuildingRealizationLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.CorrectBuildingRealization.BuildingInvalidStatus.ToTicketError(),
                BuildingHasInvalidGeometryMethodException => ValidationErrors.Common.BuildingIsMeasuredByGrb.ToTicketError(),
                BuildingHasRetiredBuildingUnitsException => ValidationErrors.CorrectBuildingRealization.BuildingHasRetiredBuildingUnits.ToTicketError(),
                _ => null
            };
        }
    }
}

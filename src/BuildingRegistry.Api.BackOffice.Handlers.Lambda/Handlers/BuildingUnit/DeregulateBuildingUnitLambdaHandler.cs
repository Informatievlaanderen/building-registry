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

    public sealed class DeregulateBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<DeregulateBuildingUnitLambdaRequest>
    {
        public DeregulateBuildingUnitLambdaHandler(
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

        protected override async Task<object> InnerHandle(DeregulateBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, DeregulateBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                BuildingUnitHasInvalidFunctionException =>
                    ValidationErrors.Common.CommonBuildingUnit.InvalidFunction.ToTicketError(),
                BuildingHasInvalidStatusException =>
                    ValidationErrors.DeregulateBuildingUnit.BuildingInvalidStatus.ToTicketError(),
                BuildingUnitHasInvalidStatusException =>
                    ValidationErrors.DeregulateBuildingUnit.BuildingUnitInvalidStatus.ToTicketError(),
                _ => null
            };
        }
    }
}

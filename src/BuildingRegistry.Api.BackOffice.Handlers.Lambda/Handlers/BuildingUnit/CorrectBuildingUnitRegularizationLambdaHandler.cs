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

    public sealed class CorrectBuildingUnitRegularizationLambdaHandler : BuildingUnitLambdaHandler<CorrectBuildingUnitRegularizationLambdaRequest>
    {
        public CorrectBuildingUnitRegularizationLambdaHandler(
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

        protected override async Task<object> InnerHandle(CorrectBuildingUnitRegularizationLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectBuildingUnitRegularizationLambdaRequest request)
        {
            return exception switch
            {
                BuildingUnitHasInvalidFunctionException =>
                    ValidationErrors.Common.CommonBuildingUnit.InvalidFunction.ToTicketError(),
                BuildingHasInvalidStatusException =>
                    ValidationErrors.CorrectBuildingUnitRegularization.BuildingInvalidStatus.ToTicketError(),
                BuildingUnitHasInvalidStatusException =>
                    ValidationErrors.CorrectBuildingUnitRegularization.BuildingUnitInvalidStatus.ToTicketError(),
                _ => null
            };
        }
    }
}

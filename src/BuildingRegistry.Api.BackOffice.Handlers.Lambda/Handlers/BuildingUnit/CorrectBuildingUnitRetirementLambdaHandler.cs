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
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class CorrectBuildingUnitRetirementLambdaHandler : BuildingUnitLambdaHandler<CorrectBuildingUnitRetirementLambdaRequest>
    {
        public CorrectBuildingUnitRetirementLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(CorrectBuildingUnitRetirementLambdaRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? InnerMapDomainException(DomainException exception, CorrectBuildingUnitRetirementLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectBuildingUnitRetirement.BuildingInvalidStatus.Message,
                    ValidationErrors.CorrectBuildingUnitRetirement.BuildingInvalidStatus.Code),

                BuildingUnitHasInvalidFunctionException => new TicketError(
                    ValidationErrors.Common.CommonBuildingUnit.Forbidden.Message,
                    ValidationErrors.Common.CommonBuildingUnit.Forbidden.Code),

                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrors.CorrectBuildingUnitRetirement.InvalidStatus.Message,
                    ValidationErrors.CorrectBuildingUnitRetirement.InvalidStatus.Code),
                _ => null
            };
        }
    }
}

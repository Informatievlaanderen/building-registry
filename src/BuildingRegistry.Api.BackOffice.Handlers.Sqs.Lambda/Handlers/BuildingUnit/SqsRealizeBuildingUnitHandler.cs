namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers.BuildingUnit
{
    using Abstractions.Building.Validators;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using TicketingService.Abstractions;

    public sealed class SqsRealizeBuildingUnitHandler : SqsLambdaBuildingUnitHandler<SqsLambdaBuildingUnitRealizeRequest>
    {
        public SqsRealizeBuildingUnitHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaBuildingUnitRealizeRequest request, CancellationToken cancellationToken)
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
                new BuildingPersistentLocalId(request.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaBuildingUnitRealizeRequest request)
        {
            return exception switch
            {
                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeRealized,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeRealized),

                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingStatusNotInRealized,
                    ValidationErrorCodes.BuildingUnit.BuildingStatusNotInRealized),
                _ => null
            };
        }
    }
}

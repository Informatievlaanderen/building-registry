namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers.BuildingUnit
{
    using Abstractions.Building.Responses;
    using Abstractions.Building.Validators;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using TicketingService.Abstractions;

    public sealed class SqsBuildingUnitNotRealizeHandler : SqsLambdaBuildingUnitHandler<SqsLambdaBuildingUnitNotRealizeRequest>
    {
        public SqsBuildingUnitNotRealizeHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaBuildingUnitNotRealizeRequest request, CancellationToken cancellationToken)
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

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaBuildingUnitNotRealizeRequest request)
        {
            return exception switch
            {
                BuildingUnitHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBeNotRealized,
                    ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBeNotRealized),
                _ => null
            };
        }
    }
}

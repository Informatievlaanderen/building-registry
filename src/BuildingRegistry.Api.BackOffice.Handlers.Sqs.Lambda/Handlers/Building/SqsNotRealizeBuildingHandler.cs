namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers.Building
{
    using Abstractions.Building.Validators;
    using Abstractions.Exceptions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Building;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using TicketingService.Abstractions;

    public sealed class SqsNotRealizeBuildingBuildingHandler : SqsLambdaBuildingHandler<SqsLambdaBuildingNotRealizeRequest>
    {
        public SqsNotRealizeBuildingBuildingHandler(
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

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaBuildingNotRealizeRequest request, CancellationToken cancellationToken)
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
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaBuildingNotRealizeRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Building.BuildingCannotBeNotRealizedException,
                    ValidationErrorCodes.Building.BuildingCannotBeNotRealizedException),
                _ => null
            };
        }
    }
}

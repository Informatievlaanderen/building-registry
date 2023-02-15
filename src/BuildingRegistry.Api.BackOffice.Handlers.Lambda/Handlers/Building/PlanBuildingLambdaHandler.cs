namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class PlanBuildingLambdaHandler : BuildingLambdaHandler<PlanBuildingLambdaRequest>
    {
        public PlanBuildingLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(PlanBuildingLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            await IdempotentCommandHandler.Dispatch(
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            var lastHash = await GetHash(request.BuildingPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, PlanBuildingLambdaRequest request)
        {
            return null;
        }
    }
}

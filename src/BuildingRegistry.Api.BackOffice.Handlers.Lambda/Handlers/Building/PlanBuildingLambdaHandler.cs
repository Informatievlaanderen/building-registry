namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class PlanBuildingLambdaHandler : BuildingLambdaHandler<PlanBuildingLambdaRequest>
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public PlanBuildingLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            IPersistentLocalIdGenerator persistentLocalIdGenerator)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        protected override async Task<ETagResponse> InnerHandle(PlanBuildingLambdaRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());
            var cmd = request.ToCommand(buildingPersistentLocalId);

            await IdempotentCommandHandler.Dispatch(
                cmd.CreateCommandId(),
                cmd,
                request.Metadata,
                cancellationToken);

            var lastHash = await GetHash(buildingPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, buildingPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, PlanBuildingLambdaRequest request)
        {
            return null;
        }
    }
}

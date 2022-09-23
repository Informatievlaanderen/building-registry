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

    public sealed class SqsPlanBuildingBuildingHandler : SqsLambdaBuildingHandler<SqsLambdaBuildingPlanRequest>
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public SqsPlanBuildingBuildingHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings, IPersistentLocalIdGenerator persistentLocalIdGenerator)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaBuildingPlanRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());
            var cmd = request.ToCommand(buildingPersistentLocalId);

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

            var lastHash = await GetHash(buildingPersistentLocalId, cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaBuildingPlanRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.Building.BuildingCannotBePlacedUnderConstruction,
                    ValidationErrorCodes.Building.BuildingCannotBePlacedUnderConstruction),
                _ => null
            };
        }
    }
}

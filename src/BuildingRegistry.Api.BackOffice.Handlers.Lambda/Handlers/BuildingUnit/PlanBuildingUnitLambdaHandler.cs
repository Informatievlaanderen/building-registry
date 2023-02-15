namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Abstractions;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class PlanBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<PlanBuildingUnitLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public PlanBuildingUnitLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            BackOfficeContext backOfficeContext)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<ETagResponse> InnerHandle(PlanBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            await using var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken);

            await IdempotentCommandHandler.Dispatch(
                               cmd.CreateCommandId(),
                               cmd,
                               request.Metadata,
                               cancellationToken);

            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(request.BuildingPersistentLocalId, request.BuildingUnitPersistentLocalId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                request.BuildingUnitPersistentLocalId,
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, PlanBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.PlanBuildingUnit.BuildingInvalidStatus.ToTicketError(),
                BuildingUnitPositionIsOutsideBuildingGeometryException => ValidationErrors.PlanBuildingUnit.BuildingUnitPositionOutsideBuildingGeometry.ToTicketError(),
                _ => null
            };
        }
    }
}

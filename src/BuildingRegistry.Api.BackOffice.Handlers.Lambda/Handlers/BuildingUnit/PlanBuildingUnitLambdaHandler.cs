namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class PlanBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<PlanBuildingUnitLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public PlanBuildingUnitLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _backOfficeContext = backOfficeContext;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        protected override async Task<ETagResponse> InnerHandle(PlanBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

            var cmd = request.ToCommand(buildingUnitPersistentLocalId);

            await using var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken);

            await IdempotentCommandHandler.Dispatch(
                               cmd.CreateCommandId(),
                               cmd,
                               request.Metadata,
                               cancellationToken);

            _backOfficeContext.BuildingUnitBuildings.Add(
                           new BuildingUnitBuilding(
                               buildingUnitPersistentLocalId,
                               request.BuildingPersistentLocalId));

            await _backOfficeContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, buildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, PlanBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBePlanned,
                        ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBePlanned),

                BuildingUnitPositionIsOutsideBuildingGeometryException => new TicketError(
                        ValidationErrorMessages.BuildingUnit.BuildingUnitOutsideGeometryBuilding,
                        ValidationErrorCodes.BuildingUnit.BuildingUnitOutsideGeometryBuilding),
                _ => null
            };
        }
    }
}

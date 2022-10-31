namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.BuildingUnit.Extensions;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using MediatR;

    public class CorrectBuildingUnitPositionHandler : BuildingUnitBusHandler, IRequestHandler<CorrectBuildingUnitPositionRequest, ETagResponse>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public CorrectBuildingUnitPositionHandler(
            ICommandHandlerResolver bus,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IdempotencyContext idempotencyContext)
            : base(bus, backOfficeContext, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<ETagResponse> Handle(CorrectBuildingUnitPositionRequest request, CancellationToken cancellationToken)
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId);
            var buildingPersistentLocalId = BackOfficeContext.GetBuildingIdForBuildingUnit(request.BuildingUnitPersistentLocalId);

            var command = request.ToCommand(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                CreateFakeProvenance());

            await using var transaction = await BackOfficeContext.Database.BeginTransactionAsync(cancellationToken);

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                command.CreateCommandId(),
                command,
                request.Metadata,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var buildingUnitLastEventHash = await GetBuildingUnitHash(buildingPersistentLocalId, buildingUnitPersistentLocalId, cancellationToken);

            return new ETagResponse(string.Empty, buildingUnitLastEventHash);
        }
    }
}

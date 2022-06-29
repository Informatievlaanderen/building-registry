namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using MediatR;

    public class SqsPlanBuildingHandler : SqsBusHandler, IRequestHandler<SqsPlanBuildingRequest, Unit>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IBuildings _buildings;

        public SqsPlanBuildingHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings)
            : base(bus)
        {
            _idempotencyContext = idempotencyContext;
            _buildings = buildings;
        }

        public async Task<Unit> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            if (!int.TryParse(request.MessageGroupId, out int buildingPersistentLocalId))
            {
                return Unit.Value;
            }
            
            var nextBuildingPersistentLocalId = new BuildingPersistentLocalId(buildingPersistentLocalId);

            var planBuilding = request.ToCommand(
                nextBuildingPersistentLocalId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                planBuilding.CreateCommandId(),
                planBuilding,
                request.Metadata,
                cancellationToken);

            var buildingHash = await GetBuildingHash(
                _buildings,
                nextBuildingPersistentLocalId,
                cancellationToken);

            // TODO: return value
            //return new PlanBuildingResponse(nextBuildingPersistentLocalId, buildingHash);
            return Unit.Value;
        }
    }
}

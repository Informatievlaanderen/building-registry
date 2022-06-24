namespace BuildingRegistry.Api.BackOffice.Building.Handlers.Sqs.Lambda.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using MediatR;

    public class SqsRealizeBuildingHandler : SqsBusHandler, IRequestHandler<SqsRealizeBuildingRequest, Unit>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IBuildings _buildings;

        public SqsRealizeBuildingHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings) : base(bus)
        {
            _idempotencyContext = idempotencyContext;
            _buildings = buildings;
        }

        public async Task<Unit> Handle(SqsRealizeBuildingRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(request.PersistentLocalId);

            var planBuilding = request.ToCommand(
                buildingPersistentLocalId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                planBuilding.CreateCommandId(),
                planBuilding,
                request.Metadata,
                cancellationToken);

            var buildingHash = await GetBuildingHash(
                _buildings,
                buildingPersistentLocalId,
                cancellationToken);

            // TODO: return value
            //return new ETagResponse(buildingHash);
            return Unit.Value;
        }
    }
}

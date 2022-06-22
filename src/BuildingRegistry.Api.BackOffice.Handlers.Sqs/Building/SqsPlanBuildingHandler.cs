namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Building;
    using MediatR;

    public class SqsPlanBuildingHandler : SqsBusHandler, IRequestHandler<SqsPlanBuildingRequest, Unit>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IBuildings _buildings;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public SqsPlanBuildingHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings,
            IPersistentLocalIdGenerator persistentLocalIdGenerator) : base(bus)
        {
            _idempotencyContext = idempotencyContext;
            _buildings = buildings;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        public async Task<Unit> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            var nextBuildingPersistentLocalId = new BuildingPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

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

            return Unit.Value;
            //return new PlanBuildingResponse(nextBuildingPersistentLocalId, buildingHash);
        }
    }
}

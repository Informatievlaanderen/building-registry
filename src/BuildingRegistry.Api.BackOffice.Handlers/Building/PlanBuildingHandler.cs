namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.Responses;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Building;
    using MediatR;

    public class PlanBuildingHandler : BuildingBusHandler, IRequestHandler<PlanBuildingRequest, PlanBuildingResponse>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public PlanBuildingHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings,
            IPersistentLocalIdGenerator persistentLocalIdGenerator) : base(bus, buildings)
        {
            _idempotencyContext = idempotencyContext;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        public async Task<PlanBuildingResponse> Handle(PlanBuildingRequest request, CancellationToken cancellationToken)
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
                nextBuildingPersistentLocalId,
                cancellationToken);

            return new PlanBuildingResponse(nextBuildingPersistentLocalId, buildingHash);
        }
    }
}

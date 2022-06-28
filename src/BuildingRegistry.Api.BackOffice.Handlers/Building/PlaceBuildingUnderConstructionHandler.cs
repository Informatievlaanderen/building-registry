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

    public class PlaceBuildingUnderConstructionHandler : BuildingBusHandler, IRequestHandler<PlaceBuildingUnderConstructionRequest, ETagResponse>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public PlaceBuildingUnderConstructionHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings) : base(bus, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<ETagResponse> Handle(PlaceBuildingUnderConstructionRequest request, CancellationToken cancellationToken)
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
                buildingPersistentLocalId,
                cancellationToken);

            return new ETagResponse(buildingHash);
        }
    }
}

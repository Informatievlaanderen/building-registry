namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.BuildingUnit.Extensions;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Building;
    using MediatR;

    public class RemoveBuildingUnitHandler : BuildingUnitBusHandler, IRequestHandler<RemoveBuildingUnitRequest>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public RemoveBuildingUnitHandler(
            ICommandHandlerResolver bus,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IdempotencyContext idempotencyContext)
            : base(bus, backOfficeContext, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<Unit> Handle(RemoveBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId);
            var buildingPersistentLocalId = BackOfficeContext.GetBuildingIdForBuildingUnit(request.BuildingUnitPersistentLocalId);

            var command = request.ToCommand(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                command.CreateCommandId(),
                command,
                request.Metadata,
                cancellationToken);

            return Unit.Value;
        }
    }
}

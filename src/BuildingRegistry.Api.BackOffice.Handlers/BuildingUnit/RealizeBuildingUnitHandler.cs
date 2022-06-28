namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Building.Responses;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Building;
    using MediatR;

    public class RealizeBuildingUnitHandler : BuildingUnitBusHandler, IRequestHandler<RealizeBuildingUnitRequest, ETagResponse>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public RealizeBuildingUnitHandler(
            ICommandHandlerResolver bus,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IdempotencyContext idempotencyContext)
            : base(bus, backOfficeContext, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<ETagResponse> Handle(RealizeBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(request.PersistentLocalId);

            if (!TryGetBuildingIdForBuildingUnit(request.PersistentLocalId, out var buildingPersistentLocalId))
            {
                throw new InvalidOperationException();
            }

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

            var buildingUnitLastEventHash = await GetBuildingUnitHash(buildingPersistentLocalId, buildingUnitPersistentLocalId, cancellationToken);

            return new ETagResponse(buildingUnitLastEventHash);
        }
    }
}

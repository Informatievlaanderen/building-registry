namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using MediatR;

    public class ChangeBuildingOutlineHandler : BuildingBusHandler, IRequestHandler<ChangeBuildingOutlineRequest, ETagResponse>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public ChangeBuildingOutlineHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings) : base(bus, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<ETagResponse> Handle(ChangeBuildingOutlineRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(request.PersistentLocalId);

            var changeBuildingOutline = request.ToCommand(
                buildingPersistentLocalId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                changeBuildingOutline.CreateCommandId(),
                changeBuildingOutline,
                request.Metadata,
                cancellationToken);

            var buildingHash = await GetBuildingHash(
                buildingPersistentLocalId,
                cancellationToken);

            return new ETagResponse(string.Empty, buildingHash);
        }
    }
}

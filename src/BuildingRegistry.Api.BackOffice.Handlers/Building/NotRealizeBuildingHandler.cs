namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.Responses;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using MediatR;

    public class NotRealizeBuildingHandler : BuildingBusHandler, IRequestHandler<NotRealizeBuildingRequest, ETagResponse>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public NotRealizeBuildingHandler(
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings) : base(bus, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<ETagResponse> Handle(NotRealizeBuildingRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(request.PersistentLocalId);

            var command = request.ToCommand(
                buildingPersistentLocalId,
                CreateFakeProvenance());

            await IdempotentCommandHandlerDispatch(
                _idempotencyContext,
                command.CreateCommandId(),
                command,
                request.Metadata,
                cancellationToken);

            var buildingHash = await GetBuildingHash(
                buildingPersistentLocalId,
                cancellationToken);

            return new ETagResponse(string.Empty, buildingHash);
        }
    }
}

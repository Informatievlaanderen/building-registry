namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Responses;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using MediatR;
    using Newtonsoft.Json;
    using TicketingService.Abstractions;

    public class SqsRealizeBuildingHandler : SqsBusHandler, IRequestHandler<SqsRealizeBuildingRequest, Unit>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IBuildings _buildings;

        public SqsRealizeBuildingHandler(
            ITicketing ticketing,
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings)
            : base(ticketing, bus)
        {
            _idempotencyContext = idempotencyContext;
            _buildings = buildings;
        }

        public async Task<Unit> Handle(SqsRealizeBuildingRequest request, CancellationToken cancellationToken)
        {
            var ticketId = request.TicketId;

            // update ticket to pending
            await Ticketing.Pending(ticketId, cancellationToken);
            
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

            // update ticket to complete
            await Ticketing.Complete(ticketId, new TicketResult(new ETagResponse(buildingHash)), cancellationToken);
            
            return Unit.Value;
        }
    }
}

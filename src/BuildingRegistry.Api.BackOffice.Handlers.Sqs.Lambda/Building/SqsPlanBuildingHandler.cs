namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Building;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using TicketingService.Abstractions;
    using static Microsoft.AspNetCore.Http.Results;

    public class SqsPlanBuildingHandler : SqsBusHandler, IRequestHandler<SqsPlanBuildingRequest, IResult>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IBuildings _buildings;

        public SqsPlanBuildingHandler(
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ICommandHandlerResolver bus,
            IdempotencyContext idempotencyContext,
            IBuildings buildings)
            : base(ticketing, ticketingUrl, bus)
        {
            _idempotencyContext = idempotencyContext;
            _buildings = buildings;
        }

        public async Task<IResult> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            var ticketId = request.TicketId;

            // update ticket to pending
            await Ticketing.Pending(ticketId, cancellationToken);

            if (!int.TryParse(request.MessageGroupId, out int buildingPersistentLocalId))
            {
                return BadRequest();
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

            // update ticket to complete
            await Ticketing.Complete(ticketId, new TicketResult(new PlanBuildingResponse(nextBuildingPersistentLocalId, buildingHash)), cancellationToken);
            
            return Ok();
        }
    }
}

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

    public class SqsPlaceBuildingUnderConstructionHandler : SqsBusHandler, IRequestHandler<SqsPlaceBuildingUnderConstructionRequest, IResult>
    {
        private readonly IdempotencyContext _idempotencyContext;
        private readonly IBuildings _buildings;

        public SqsPlaceBuildingUnderConstructionHandler(
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

        public async Task<IResult> Handle(SqsPlaceBuildingUnderConstructionRequest request, CancellationToken cancellationToken)
        {
            // update ticket to pending
            await Ticketing.Pending(request.TicketId, cancellationToken);

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
            await Ticketing.Complete(request.TicketId, new TicketResult(new ETagResponse(buildingHash)), cancellationToken);

            var location = TicketingUrl.For(request.TicketId);
            return Accepted(location);
        }
    }
}

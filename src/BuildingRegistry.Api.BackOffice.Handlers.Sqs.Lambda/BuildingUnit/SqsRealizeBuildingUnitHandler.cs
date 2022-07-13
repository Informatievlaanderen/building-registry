namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.BuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Extensions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using MediatR;
    using TicketingService.Abstractions;

    public class SqsRealizeBuildingUnitHandler : SqsBuildingUnitBusHandler, IRequestHandler<SqsRealizeBuildingUnitRequest, Unit>
    {
        private readonly IdempotencyContext _idempotencyContext;

        public SqsRealizeBuildingUnitHandler(
            ITicketing ticketing,
            ICommandHandlerResolver bus,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IdempotencyContext idempotencyContext)
            : base(ticketing, bus, backOfficeContext, buildings)
        {
            _idempotencyContext = idempotencyContext;
        }

        public async Task<Unit> Handle(SqsRealizeBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            var ticketId = request.TicketId;

            // update ticket to pending
            await Ticketing.Pending(ticketId, cancellationToken);

            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(request.PersistentLocalId);

            if (!request.PersistentLocalId.TryGetBuildingIdForBuildingUnit(BackOfficeContext, out var buildingPersistentLocalId))
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

            // update ticket to complete
            await Ticketing.Complete(ticketId, new TicketResult(new ETagResponse(buildingUnitLastEventHash)), cancellationToken);

            return Unit.Value;
        }
    }
}

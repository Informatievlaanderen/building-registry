namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Responses;
    using BuildingRegistry.Building;
    using MediatR;

    public class SqsPlanBuildingUnitHandler : SqsBuildingUnitBusHandler, IRequestHandler<SqsPlanBuildingUnitRequest, Unit>
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly IdempotencyContext _idempotencyContext;

        public SqsPlanBuildingUnitHandler(
            ITicketing ticketing,
            ICommandHandlerResolver bus,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IdempotencyContext idempotencyContext)
            : base(ticketing, bus, backOfficeContext, buildings)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _idempotencyContext = idempotencyContext;
        }

        public async Task<Unit> Handle(SqsPlanBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            var ticketId = request.TicketId;

            // update ticket to pending
            await Ticketing.Pending(ticketId);

            var buildingPersistentLocalId = new BuildingPersistentLocalId(OsloPuriValidator.ParsePersistentLocalId(request.GebouwId));
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

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

            BackOfficeContext.BuildingUnitBuildings.Add(
                new BuildingUnitBuilding(
                    buildingUnitPersistentLocalId,
                    buildingPersistentLocalId));
            await BackOfficeContext.SaveChangesAsync(cancellationToken);

            var buildingUnitLastEventHash = await GetBuildingUnitHash(buildingPersistentLocalId, buildingUnitPersistentLocalId, cancellationToken);

            // update ticket to complete
            await Ticketing.Complete(ticketId, new PlanBuildingUnitResponse(buildingUnitPersistentLocalId, buildingUnitLastEventHash));

            return Unit.Value;
        }
    }
}

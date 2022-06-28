namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Building.Validators;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.Responses;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using BuildingRegistry.Building;
    using MediatR;

    public class PlanBuildingUnitHandler : BuildingUnitBusHandler, IRequestHandler<PlanBuildingUnitRequest, PlanBuildingUnitResponse>
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly IdempotencyContext _idempotencyContext;

        public PlanBuildingUnitHandler(
            ICommandHandlerResolver bus,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            IdempotencyContext idempotencyContext)
            : base(bus, backOfficeContext, buildings)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _idempotencyContext = idempotencyContext;
        }

        public async Task<PlanBuildingUnitResponse> Handle(PlanBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId =  new BuildingPersistentLocalId(OsloPuriValidator.ParsePersistentLocalId(request.GebouwId));
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
            return new PlanBuildingUnitResponse(buildingPersistentLocalId, buildingUnitPersistentLocalId, buildingUnitLastEventHash);
        }
    }
}

namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;

    public abstract class SqsBuildingUnitBusHandler : SqsBusHandler
    {
        protected IBuildings Buildings { get; }
        protected BackOfficeContext BackOfficeContext { get; }

        protected SqsBuildingUnitBusHandler(
            ICommandHandlerResolver bus,
            BackOfficeContext backOfficeContext,
            IBuildings buildings) : base(bus)
        {
            Buildings = buildings;
            BackOfficeContext = backOfficeContext;
        }

        protected async Task<string> GetBuildingUnitHash(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var building = await Buildings.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);

            var buildingUnit = building.BuildingUnits.FirstOrDefault(
                x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException();
            }

            return buildingUnit.LastEventHash;
        }
    }
}

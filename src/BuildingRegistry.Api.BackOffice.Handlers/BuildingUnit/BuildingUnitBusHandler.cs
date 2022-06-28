namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;

    public abstract class BuildingUnitBusHandler : BusHandler
    {
        protected IBuildings Buildings { get; }
        protected BackOfficeContext BackOfficeContext { get; }

        protected BuildingUnitBusHandler(
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
            var building =
                await Buildings.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);

            var buildingUnit = building.BuildingUnits.FirstOrDefault(
                x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException();
            }

            return buildingUnit.LastEventHash;
        }

        protected bool TryGetBuildingIdForBuildingUnit(int buildingUnitPersistentLocalId, out BuildingPersistentLocalId buildingPersistentLocalId)
        {
            buildingPersistentLocalId = null;

            var buildingUnitBuilding = BackOfficeContext.BuildingUnitBuildings
                .Find(buildingUnitPersistentLocalId);

            if (buildingUnitBuilding is null)
            {
                return false;
            }

            buildingPersistentLocalId = new BuildingPersistentLocalId(buildingUnitBuilding.BuildingPersistentLocalId);
            return true;
        }
    }
}

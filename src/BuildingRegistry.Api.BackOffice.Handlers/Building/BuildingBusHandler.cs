namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using BuildingRegistry.Building;

    public abstract class BuildingBusHandler : BusHandler
    {
        protected IBuildings Buildings { get; }

        public BuildingBusHandler(ICommandHandlerResolver bus, IBuildings buildings) : base(bus)
        {
            Buildings = buildings;
        }

        protected async Task<string> GetBuildingHash(
            BuildingPersistentLocalId buildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await Buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), cancellationToken);
            return aggregate.LastEventHash;
        }
    }
}

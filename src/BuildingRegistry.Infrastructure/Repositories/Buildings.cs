namespace BuildingRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Building;
    using SqlStreamStore;

    public class Buildings : Repository<Building>, IBuildings
    {
        public Buildings(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(Building.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer)
        {
        }
    }
}

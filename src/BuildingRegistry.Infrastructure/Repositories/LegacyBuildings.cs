namespace BuildingRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Legacy;
    using SqlStreamStore;

    public class LegacyBuildings : Repository<Building>, IBuildings
    {
        public LegacyBuildings(ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(Building.Factory, unitOfWork, eventStore, eventMapping, eventDeserializer)
        {
        }
    }
}

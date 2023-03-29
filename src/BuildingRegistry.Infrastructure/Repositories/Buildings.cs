namespace BuildingRegistry.Infrastructure.Repositories
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Building;
    using SqlStreamStore;

    public class Buildings : Repository<Building, BuildingStreamId>, IBuildings
    {
        public Buildings(IBuildingFactory buildingFactory, ConcurrentUnitOfWork unitOfWork, IStreamStore eventStore, ISnapshotStore snapshotStore, EventMapping eventMapping, EventDeserializer eventDeserializer)
            : base(buildingFactory.Create, unitOfWork, eventStore, snapshotStore, eventMapping, eventDeserializer)
        { }
    }
}

namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;

    public interface IBuildingFactory
    {
        Building Create();
    }

    public class BuildingFactory : IBuildingFactory
    {
        private readonly ISnapshotStrategy _snapshotStrategy;

        public BuildingFactory(ISnapshotStrategy snapshotStrategy)
        {
            _snapshotStrategy = snapshotStrategy;
        }

        public Building Create()
        {
            return new Building(_snapshotStrategy);
        }
    }
}

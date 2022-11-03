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
        private readonly IAddCommonBuildingUnit _addCommonBuildingUnit;

        public BuildingFactory(ISnapshotStrategy snapshotStrategy, IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            _snapshotStrategy = snapshotStrategy;
            _addCommonBuildingUnit = addCommonBuildingUnit;
        }

        public Building Create()
        {
            return new Building(_snapshotStrategy, _addCommonBuildingUnit);
        }
    }
}

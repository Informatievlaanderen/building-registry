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
        private readonly IAddresses _addresses;

        public BuildingFactory(ISnapshotStrategy snapshotStrategy, IAddCommonBuildingUnit addCommonBuildingUnit, IAddresses addresses)
        {
            _snapshotStrategy = snapshotStrategy;
            _addCommonBuildingUnit = addCommonBuildingUnit;
            _addresses = addresses;
        }

        public Building Create()
        {
            return new Building(_snapshotStrategy, _addCommonBuildingUnit, _addresses);
        }
    }
}

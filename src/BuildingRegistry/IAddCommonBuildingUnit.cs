namespace BuildingRegistry
{
    using Building;

    public interface IAddCommonBuildingUnit : IPersistentLocalIdGenerator
    {
        void AddForBuilding(BuildingPersistentLocalId buildingPersistentLocalId, BuildingUnitPersistentLocalId buildingUnitPersistentLocalId);
    }
}

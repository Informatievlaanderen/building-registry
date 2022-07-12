namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingUnitNotFoundException : BuildingRegistryException
    {
        public BuildingUnitNotFoundException()
        { }

        public BuildingUnitNotFoundException(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId
            )  : base($"BuildingUnit with id '{buildingUnitPersistentLocalId}' was not found in Building '{buildingPersistentLocalId}'.") { }
    }
}

namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingUnitIsRemovedException : BuildingRegistryException
    {
        public BuildingUnitIsRemovedException(int persistentLocalId) : base($"BuildingUnit with Id '{persistentLocalId}' is removed.") { }
    }
}

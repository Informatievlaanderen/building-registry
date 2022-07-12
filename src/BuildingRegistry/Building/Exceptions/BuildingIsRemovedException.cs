namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingIsRemovedException : BuildingRegistryException
    {
        public BuildingIsRemovedException(int persistentLocalId) : base($"Building with Id '{persistentLocalId}' is removed.") { }
    }
}

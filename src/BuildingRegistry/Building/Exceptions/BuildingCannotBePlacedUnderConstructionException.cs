namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingCannotBePlacedUnderConstructionException : BuildingRegistryException
    {
        public BuildingCannotBePlacedUnderConstructionException(BuildingStatus status) : base($"Cannot put building with status '{status}' under construction.") { }
    }
}

namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingCannotBeRealizedException : BuildingRegistryException
    {
        public BuildingCannotBeRealizedException(BuildingStatus status) : base($"Cannot realize building with status '{status}'.") { }
    }
}

namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingCannotBeRealizedException : BuildingRegistryException
    {
        public BuildingCannotBeRealizedException() { }

        public BuildingCannotBeRealizedException(string message) : base(message) { }
    }
}

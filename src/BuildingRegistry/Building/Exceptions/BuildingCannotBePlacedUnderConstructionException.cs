namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingCannotBePlacedUnderConstructionException : BuildingRegistryException
    {
        public BuildingCannotBePlacedUnderConstructionException(string message) : base(message) { }
    }
}

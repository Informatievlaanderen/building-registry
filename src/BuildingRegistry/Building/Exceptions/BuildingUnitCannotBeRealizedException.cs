namespace BuildingRegistry.Building.Exceptions
{
    public class BuildingUnitCannotBeRealizedException : BuildingRegistryException
    {
        public BuildingUnitCannotBeRealizedException(BuildingUnitStatus status) : base($"Cannot realize buildingUnit with status '{status}'.") { }
    }
}

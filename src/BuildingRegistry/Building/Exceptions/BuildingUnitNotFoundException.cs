namespace BuildingRegistry.Building.Exceptions
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingUnitNotFoundException : DomainException
    {
        public BuildingUnitNotFoundException()
        { }

        public BuildingUnitNotFoundException(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId
            )  : base($"BuildingUnit with id '{buildingUnitPersistentLocalId}' was not found in Building '{buildingPersistentLocalId}'.") { }
    }
}

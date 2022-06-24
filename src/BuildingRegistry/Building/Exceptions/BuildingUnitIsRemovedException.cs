namespace BuildingRegistry.Building.Exceptions
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingUnitIsRemovedException : DomainException
    {
        public BuildingUnitIsRemovedException(int persistentLocalId) : base($"BuildingUnit with Id '{persistentLocalId}' is removed.") { }
    }
}

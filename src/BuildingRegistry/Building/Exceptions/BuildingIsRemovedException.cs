namespace BuildingRegistry.Building.Exceptions
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class BuildingIsRemovedException : DomainException
    {
        public BuildingIsRemovedException(int persistentLocalId) : base($"Building with Id '{persistentLocalId}' is removed.") { }
    }
}

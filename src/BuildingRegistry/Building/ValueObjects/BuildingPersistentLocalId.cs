namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class BuildingPersistentLocalId : IntegerValueObject<BuildingPersistentLocalId>
    {
        public BuildingPersistentLocalId(int persistentLocalId) : base(persistentLocalId) { }
    }
}

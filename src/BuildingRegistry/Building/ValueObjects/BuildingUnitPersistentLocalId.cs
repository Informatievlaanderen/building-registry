namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class BuildingUnitPersistentLocalId : IntegerValueObject<BuildingUnitPersistentLocalId>
    {
        public BuildingUnitPersistentLocalId(int persistentLocalId) : base(persistentLocalId) { }
    }
}

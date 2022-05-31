namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class BuildingUnitPersistentLocalId : IntegerValueObject<BuildingUnitPersistentLocalId>
    {
        public BuildingUnitPersistentLocalId([JsonProperty("value")] int persistentLocalId) : base(persistentLocalId) { }
    }
}

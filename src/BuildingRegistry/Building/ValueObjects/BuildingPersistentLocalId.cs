namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class BuildingPersistentLocalId : IntegerValueObject<BuildingPersistentLocalId>
    {
        public BuildingPersistentLocalId([JsonProperty("value")] int persistentLocalId) : base(persistentLocalId) { }
    }
}

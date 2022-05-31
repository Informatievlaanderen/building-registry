namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class AddressPersistentLocalId : IntegerValueObject<AddressPersistentLocalId>
    {
        public AddressPersistentLocalId([JsonProperty("value")] int persistentLocalId) : base(persistentLocalId) { }
    }
}

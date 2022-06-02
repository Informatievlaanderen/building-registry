namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class AddressPersistentLocalId : IntegerValueObject<AddressPersistentLocalId>
    {
        public AddressPersistentLocalId(int persistentLocalId) : base(persistentLocalId) { }
    }
}

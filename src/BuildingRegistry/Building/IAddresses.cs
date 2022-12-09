namespace BuildingRegistry.Building
{
    using Datastructures;

    public interface IAddresses
    {
        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId);
    }
}

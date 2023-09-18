namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Datastructures;

    public interface IAddresses
    {
        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId);
        public Task<List<AddressData>> GetAddresses(List<AddressPersistentLocalId> addressPersistentLocalIds);
    }
}

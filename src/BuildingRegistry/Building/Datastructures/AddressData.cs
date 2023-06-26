namespace BuildingRegistry.Building.Datastructures
{
    public record struct AddressData(AddressPersistentLocalId AddressPersistentLocalId, AddressStatus Status,
        bool IsRemoved);

    public enum AddressStatus
    {
        Current,
        Proposed,
        Retired,
        Rejected
    }
}

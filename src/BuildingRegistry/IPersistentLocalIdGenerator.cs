namespace BuildingRegistry
{
    using ValueObjects;

    public interface IPersistentLocalIdGenerator
    {
        PersistentLocalId GenerateNextPersistentLocalId();
    }
}

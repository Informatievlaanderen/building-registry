namespace BuildingRegistry
{
    using Legacy;

    public interface IPersistentLocalIdGenerator
    {
        PersistentLocalId GenerateNextPersistentLocalId();
    }
}

namespace BuildingRegistry.Tests.Legacy
{
    using BuildingRegistry.Legacy;

    public class FakePersistentLocalIdGenerator : IPersistentLocalIdGenerator
    {
        public PersistentLocalId GenerateNextPersistentLocalId()
        {
            return new PersistentLocalId(1);
        }
    }
}

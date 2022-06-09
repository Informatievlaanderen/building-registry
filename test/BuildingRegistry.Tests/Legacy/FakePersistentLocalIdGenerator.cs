namespace BuildingRegistry.Tests.Legacy
{
    public class FakePersistentLocalIdGenerator : IPersistentLocalIdGenerator
    {
        public int GenerateNextPersistentLocalId() => 1;
    }
}

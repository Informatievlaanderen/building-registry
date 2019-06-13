namespace BuildingRegistry.Tests
{
    using ValueObjects;

    public class FakeOsloIdGenerator : IOsloIdGenerator
    {
        public OsloId GenerateNextOsloId()
        {
            return new OsloId(1);
        }
    }
}

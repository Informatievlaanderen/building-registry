namespace BuildingRegistry
{
    using ValueObjects;

    public interface IOsloIdGenerator
    {
        OsloId GenerateNextOsloId();
    }
}

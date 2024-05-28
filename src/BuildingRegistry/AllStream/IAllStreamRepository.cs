namespace BuildingRegistry.AllStream
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public interface IAllStreamRepository : IAsyncRepository<AllStream, AllStreamId>
    {
    }
}

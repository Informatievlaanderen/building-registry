namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Obsolete("This is a legacy class and should not be used anymore.")]
    public interface IBuildings : IAsyncRepository<Building>
    {
    }
}

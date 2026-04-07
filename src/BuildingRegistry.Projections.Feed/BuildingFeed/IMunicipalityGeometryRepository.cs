namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using System.Collections.Generic;
    using NodaTime;

    public interface IMunicipalityGeometryRepository
    {
        List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex, Instant eventTimestamp);
    }
}

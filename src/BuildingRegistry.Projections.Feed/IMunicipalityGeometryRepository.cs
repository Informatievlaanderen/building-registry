namespace BuildingRegistry.Projections.Feed
{
    using System.Collections.Generic;
    using NodaTime;

    public interface IMunicipalityGeometryRepository
    {
        List<string> GetOverlappingNisCodes(string extendedWkbGeometryAsHex, Instant eventTimestamp);
    }
}

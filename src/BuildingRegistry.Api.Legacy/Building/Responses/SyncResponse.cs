namespace BuildingRegistry.Api.Legacy.Abstractions.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Query;

    public record SyncResponse(DateTimeOffset LastFeedUpdate, PagedQueryable<BuildingSyndicationQueryResult> PagedBuildings);
}

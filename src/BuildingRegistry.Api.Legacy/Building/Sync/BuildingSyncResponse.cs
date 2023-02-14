namespace BuildingRegistry.Api.Legacy.Building.Sync
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using BuildingRegistry.Api.Legacy.Building.Query;

    public record SyncResponse(DateTimeOffset LastFeedUpdate, PagedQueryable<BuildingSyndicationQueryResult> PagedBuildings);
}

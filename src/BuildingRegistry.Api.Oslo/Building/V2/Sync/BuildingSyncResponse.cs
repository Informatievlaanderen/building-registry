namespace BuildingRegistry.Api.Oslo.Building.V2.Sync
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Query;

    public record SyncResponse(DateTimeOffset LastFeedUpdate, PagedQueryable<BuildingSyndicationQueryResult> PagedBuildings);
}

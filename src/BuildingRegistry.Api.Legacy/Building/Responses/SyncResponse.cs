namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Query;

    public record SyncResponse(DateTimeOffset LastFeedUpdate, PagedQueryable<BuildingSyndicationQueryResult> PagedBuildings);
}

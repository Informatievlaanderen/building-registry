namespace BuildingRegistry.Api.Oslo.Building.Sync
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public record SyncRequest(
        FilteringHeader<BuildingSyndicationFilter> FilteringHeader,
        SortingHeader SortingHeader,
        IPaginationRequest PaginationRequest)
        : IRequest<SyncResponse>;
}

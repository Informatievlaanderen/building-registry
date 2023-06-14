namespace BuildingRegistry.Api.Oslo.Building.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public record BuildingListRequest(
        FilteringHeader<BuildingFilter> FilteringHeader,
        SortingHeader SortingHeader,
        IPaginationRequest PaginationRequest)
        : IRequest<BuildingListOsloResponse>;
}

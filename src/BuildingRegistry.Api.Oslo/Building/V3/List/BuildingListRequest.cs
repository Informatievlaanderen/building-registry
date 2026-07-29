namespace BuildingRegistry.Api.Oslo.Building.V3.List
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
        : IRequest<BuildingListOsloV3Response>;
}

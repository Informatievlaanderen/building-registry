namespace BuildingRegistry.Api.Legacy.BuildingUnit.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public record BuildingUnitListRequest(
        FilteringHeader<BuildingUnitFilter> FilteringHeader,
        SortingHeader SortingHeader,
        IPaginationRequest PaginationRequest)
        : IRequest<BuildingUnitListResponse>;
}

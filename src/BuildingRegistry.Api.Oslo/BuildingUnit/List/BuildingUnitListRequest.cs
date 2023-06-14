namespace BuildingRegistry.Api.Oslo.BuildingUnit.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using MediatR;
    using Query;

    public record ListRequest(
            FilteringHeader<BuildingUnitFilter> FilteringHeader,
            SortingHeader SortingHeader,
            IPaginationRequest PaginationRequest)
        : IRequest<BuildingUnitListOsloResponse>;
}

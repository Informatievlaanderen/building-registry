namespace BuildingRegistry.Api.Legacy.Building.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Query;

    public record ListRequest(
        LegacyContext Context,
        IOptions<ResponseOptions> ResponseOptions,
        FilteringHeader<BuildingFilter> FilteringHeader,
        SortingHeader SortingHeader,
        IPaginationRequest PaginationRequest,
        HttpResponse HttpResponse
        ) : IRequest<BuildingListResponse>;
}

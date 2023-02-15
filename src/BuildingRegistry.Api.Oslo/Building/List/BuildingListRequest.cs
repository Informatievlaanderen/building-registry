namespace BuildingRegistry.Api.Oslo.Building.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using BuildingRegistry.Api.Oslo.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Query;

    public record BuildingListRequest(
        FilteringHeader<BuildingFilter> FilteringHeader,
        SortingHeader SortingHeader,
        IPaginationRequest PaginationRequest,
        HttpResponse HttpResponse,
        LegacyContext Context,
        IOptions<ResponseOptions> ResponseOptions
    ) : IRequest<BuildingListOsloResponse>;
}

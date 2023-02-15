namespace BuildingRegistry.Api.Legacy.BuildingUnit.List
{
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using BuildingRegistry.Api.Legacy.Infrastructure.Options;
    using BuildingRegistry.Projections.Legacy;
    using BuildingRegistry.Projections.Syndication;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Query;

    public record BuildingUnitListRequest(
        LegacyContext Context,
        SyndicationContext SyndicationContext,
        IOptions<ResponseOptions> ResponseOptions,
        FilteringHeader<BuildingUnitFilter> FilteringHeader,
        SortingHeader SortingHeader,
        IPaginationRequest PaginationRequest,
        HttpResponse HttpResponse
            ) : IRequest<BuildingUnitListResponse>;
}

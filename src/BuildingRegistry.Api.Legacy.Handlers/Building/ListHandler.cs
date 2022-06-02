namespace BuildingRegistry.Api.Legacy.Handlers.Building
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building;
    using Abstractions.Infrastructure;
    using Api.Legacy.Abstractions.Building.Query;
    using Api.Legacy.Abstractions.Building.Responses;
    using Api.Legacy.Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public record ListRequest(LegacyContext Context, IOptions<ResponseOptions> ResponseOptions, HttpRequest HttpRequest, HttpResponse HttpResponse) : IRequest<BuildingListResponse>;

    public class ListHandler : IRequestHandler<ListRequest, BuildingListResponse>
    {
        public async Task<BuildingListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedBuildings = new BuildingListQuery(request.Context)
                .Fetch(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedBuildings);

            var buildings = await pagedBuildings.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            return new BuildingListResponse
            {
                Gebouwen = buildings
                    .Select(x => new GebouwCollectieItem(
                        x.PersistentLocalId.Value,
                        request.ResponseOptions.Value.GebouwNaamruimte,
                        request.ResponseOptions.Value.GebouwDetailUrl,
                        BuildingHelpers.MapBuildingStatus(x.Status.Value),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildings.PaginationInfo.BuildNextUri(request.ResponseOptions.Value.GebouwVolgendeUrl)
            };
        }
    }
}

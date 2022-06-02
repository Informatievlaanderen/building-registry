namespace BuildingRegistry.Api.Legacy.Handlers.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit;
    using Abstractions.Infrastructure;
    using Api.Legacy.Abstractions.BuildingUnit.Query;
    using Api.Legacy.Abstractions.BuildingUnit.Responses;
    using Api.Legacy.Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Projections.Legacy;
    using Projections.Syndication;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;


    public record ListRequest(LegacyContext Context, SyndicationContext SyndicationContext, IOptions<ResponseOptions> ResponseOptions, HttpRequest HttpRequest, HttpResponse HttpResponse) : IRequest<BuildingUnitListResponse>;

    public class ListHandler : IRequestHandler<ListRequest, BuildingUnitListResponse>
    {
        public async Task<BuildingUnitListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedBuildingUnits = new BuildingUnitListQuery(request.Context, request.SyndicationContext)
                .Fetch(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedBuildingUnits);

            var units = await pagedBuildingUnits.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            return new BuildingUnitListResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItem(
                        x.PersistentLocalId.Value,
                        request.ResponseOptions.Value.GebouweenheidNaamruimte,
                        request.ResponseOptions.Value.GebouweenheidDetailUrl,
                        BuildingUnitHelpers.MapBuildingUnitStatus(x.Status.Value),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(request.ResponseOptions.Value.GebouweenheidVolgendeUrl)
            };
        }
    }
}

namespace BuildingRegistry.Api.Legacy.Handlers.BuildingUnitV2
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit;
    using Abstractions.BuildingUnit.Query;
    using Abstractions.BuildingUnit.Responses;
    using Abstractions.Converters;
    using Abstractions.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ListHandler : IRequestHandler<ListRequest, BuildingUnitListResponse>
    {
        public async Task<BuildingUnitListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingUnitFilterV2>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedBuildingUnits = new BuildingUnitListQueryV2(request.Context, request.SyndicationContext)
                .Fetch(filtering, sorting, pagination);

            request.HttpResponse.AddPagedQueryResultHeaders(pagedBuildingUnits);

            var units = await pagedBuildingUnits.Items
                .Select(a => new
                {
                    a.BuildingUnitPersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            return new BuildingUnitListResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItem(
                        x.BuildingUnitPersistentLocalId,
                        request.ResponseOptions.Value.GebouweenheidNaamruimte,
                        request.ResponseOptions.Value.GebouweenheidDetailUrl,
                        x.Status.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(request.ResponseOptions.Value.GebouweenheidVolgendeUrl)
            };
        }
    }
}

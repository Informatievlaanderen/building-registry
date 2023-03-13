namespace BuildingRegistry.Api.Legacy.BuildingUnit.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using BuildingRegistry.Api.Legacy.BuildingUnit.Query;
    using BuildingRegistry.Api.Legacy.Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuildingUnitListHandlerV2 : IRequestHandler<BuildingUnitListRequest, BuildingUnitListResponse>
    {
        public async Task<BuildingUnitListResponse> Handle(BuildingUnitListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildingUnits = new BuildingUnitListQueryV2(request.Context)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

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
                    .BuildNextUri(units.Count, request.ResponseOptions.Value.GebouweenheidVolgendeUrl)
            };
        }
    }
}

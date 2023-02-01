namespace BuildingRegistry.Api.Oslo.BuildingUnit.HandlersV2
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Query;
    using Requests;
    using Responses;

    public class ListHandler : IRequestHandler<ListRequest, BuildingUnitListOsloResponse>
    {
        public async Task<BuildingUnitListOsloResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingUnitFilterV2>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedBuildingUnits = new BuildingUnitListOsloQueryV2(request.Context)
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

            return new BuildingUnitListOsloResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItemOslo(
                        x.BuildingUnitPersistentLocalId,
                        request.ResponseOptions.Value.GebouweenheidNaamruimte,
                        request.ResponseOptions.Value.GebouweenheidDetailUrl,
                        x.Status.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(request.ResponseOptions.Value.GebouweenheidVolgendeUrl),
                Context = request.ResponseOptions.Value.ContextUrlUnitList
            };
        }
    }
}

namespace BuildingRegistry.Api.Oslo.BuildingUnit.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using BuildingRegistry.Api.Oslo.BuildingUnit.Query;
    using BuildingRegistry.Api.Oslo.Converters;
    using BuildingRegistry.Api.Oslo.Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ListHandler : IRequestHandler<ListRequest, BuildingUnitListOsloResponse>
    {
        public async Task<BuildingUnitListOsloResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

            var pagedBuildingUnits = new BuildingUnitListOsloQuery(request.Context, request.SyndicationContext)
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

            return new BuildingUnitListOsloResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItemOslo(
                        x.PersistentLocalId.Value,
                        request.ResponseOptions.Value.GebouweenheidNaamruimte,
                        request.ResponseOptions.Value.GebouweenheidDetailUrl,
                        x.Status.Value.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(units.Count, request.ResponseOptions.Value.GebouweenheidVolgendeUrl),
                Context = request.ResponseOptions.Value.ContextUrlUnitList
            };
        }
    }
}
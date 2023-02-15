namespace BuildingRegistry.Api.Legacy.Building.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using BuildingRegistry.Api.Legacy.Building.Query;
    using BuildingRegistry.Api.Legacy.Infrastructure;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class ListHandler : IRequestHandler<ListRequest, BuildingListResponse>
    {
        public async Task<BuildingListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildings = new BuildingListQuery(request.Context)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

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
                        x.Status.Value.ConvertFromBuildingStatus(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildings.PaginationInfo.BuildNextUri(request.ResponseOptions.Value.GebouwVolgendeUrl)
            };
        }
    }
}

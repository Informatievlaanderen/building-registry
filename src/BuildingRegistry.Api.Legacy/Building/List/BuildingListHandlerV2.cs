namespace BuildingRegistry.Api.Legacy.Building.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Query;

    public class ListHandlerV2 : IRequestHandler<ListRequest, BuildingListResponse>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public ListHandlerV2(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingListResponse> Handle(ListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildings = new BuildingListQueryV2(_context)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

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
                        x.PersistentLocalId,
                        _responseOptions.Value.GebouwNaamruimte,
                        _responseOptions.Value.GebouwDetailUrl,
                        x.Status.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildings.PaginationInfo.BuildNextUri(buildings.Count, _responseOptions.Value.GebouwVolgendeUrl),
                Sorting = pagedBuildings.Sorting,
                Pagination = pagedBuildings.PaginationInfo
            };
        }
    }
}

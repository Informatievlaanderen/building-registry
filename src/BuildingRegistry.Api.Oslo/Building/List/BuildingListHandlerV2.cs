namespace BuildingRegistry.Api.Oslo.Building.List
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Converters;
    using Infrastructure;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Query;

    public class BuildingListHandlerV2 : IRequestHandler<BuildingListRequest, BuildingListOsloResponse>
    {
        private readonly LegacyContext _context;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public BuildingListHandlerV2(
            LegacyContext context,
            IOptions<ResponseOptions> responseOptions)
        {
            _context = context;
            _responseOptions = responseOptions;
        }

        public async Task<BuildingListOsloResponse> Handle(BuildingListRequest request, CancellationToken cancellationToken)
        {
            var pagedBuildings = new BuildingListOsloQueryV2(_context)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

            var buildings = await pagedBuildings.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            return new BuildingListOsloResponse
            {
                Gebouwen = buildings
                    .Select(x => new GebouwCollectieItemOslo(
                        x.PersistentLocalId,
                        _responseOptions.Value.GebouwNaamruimte,
                        _responseOptions.Value.GebouwDetailUrl,
                        x.Status.Map(),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildings.PaginationInfo.BuildNextUri(buildings.Count, _responseOptions.Value.GebouwVolgendeUrl),
                Context = _responseOptions.Value.ContextUrlList,
                Sorting = pagedBuildings.Sorting,
                Pagination = pagedBuildings.PaginationInfo
            };
        }
    }
}

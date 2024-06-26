namespace BuildingRegistry.Api.Oslo.Building.Sync
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Projections.Legacy;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Query;

    public class BuildingSyncHandler : IRequestHandler<SyncRequest, SyncResponse>
    {
        private readonly LegacyContext _context;

        public BuildingSyncHandler(
            LegacyContext context)
        {
            _context = context;
        }

        public async Task<SyncResponse> Handle(SyncRequest request, CancellationToken cancellationToken)
        {
            var lastFeedUpdate = await _context
                .BuildingSyndicationWithCount
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
            {
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }

            var pagedBuildings = new BuildingSyndicationQuery(
                    _context,
                    request.FilteringHeader.Filter?.Embed)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

            return new SyncResponse(lastFeedUpdate, pagedBuildings);
        }
    }
}

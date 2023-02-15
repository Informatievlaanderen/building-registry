namespace BuildingRegistry.Api.Legacy.Building.Sync
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using BuildingRegistry.Api.Legacy.Building.Query;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BuildingSyncHandler : IRequestHandler<SyncRequest, SyncResponse>
    {
        public async Task<SyncResponse> Handle(SyncRequest request, CancellationToken cancellationToken)
        {
            var lastFeedUpdate = await request.Context
                .BuildingSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
            {
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }

            var pagedBuildings = new BuildingSyndicationQuery(
                    request.Context,
                    request.FilteringHeader.Filter.Embed)
                .Fetch(request.FilteringHeader, request.SortingHeader, request.PaginationRequest);

            return new SyncResponse(lastFeedUpdate, pagedBuildings);
        }
    }
}

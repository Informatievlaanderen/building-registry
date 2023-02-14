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
            var filtering = request.HttpRequest.ExtractFilteringRequest<BuildingSyndicationFilter>();
            var sorting = request.HttpRequest.ExtractSortingRequest();
            var pagination = request.HttpRequest.ExtractPaginationRequest();

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
                    filtering.Filter?.Embed)
                .Fetch(filtering, sorting, pagination);

            return new SyncResponse(lastFeedUpdate, pagedBuildings);
        }
    }
}

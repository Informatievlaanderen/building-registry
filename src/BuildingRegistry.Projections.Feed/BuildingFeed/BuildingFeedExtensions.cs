namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingFeedExtensions
    {
        public static async Task<int> CalculatePage(this FeedContext context, int maxPageSize = ChangeFeedService.DefaultMaxPageSize)
        {
            if (!await context.BuildingFeed.AnyAsync() && context.BuildingFeed.Local.Count == 0)
            {
                return 1;
            }

            var maxPage = await context.BuildingFeed.MaxAsync(x => x.Page);
            var dbCount = await context.BuildingFeed.CountAsync(x => x.Page == maxPage);

            var localCount = context.BuildingFeed.Local
                .Count(x => x.Page == maxPage && context.Entry(x).State == EntityState.Added);

            var totalCount = dbCount + localCount;

            return totalCount >= maxPageSize ? maxPage + 1 : maxPage;
        }
    }
}

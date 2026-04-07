namespace BuildingRegistry.Projections.Feed.BuildingUnitFeed
{
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingUnitFeedExtensions
    {
        public static async Task<int> CalculateBuildingUnitPage(this FeedContext context, int maxPageSize = ChangeFeedService.DefaultMaxPageSize)
        {
            if (!await context.BuildingUnitFeed.AnyAsync() && context.BuildingUnitFeed.Local.Count == 0)
            {
                return 1;
            }

            var maxPage = await context.BuildingUnitFeed.MaxAsync(x => x.Page);
            var dbCount = await context.BuildingUnitFeed.CountAsync(x => x.Page == maxPage);

            var localCount = context.BuildingUnitFeed.Local
                .Count(x => x.Page == maxPage && context.Entry(x).State == EntityState.Added);

            var totalCount = dbCount + localCount;

            return totalCount >= maxPageSize ? maxPage + 1 : maxPage;
        }
    }
}

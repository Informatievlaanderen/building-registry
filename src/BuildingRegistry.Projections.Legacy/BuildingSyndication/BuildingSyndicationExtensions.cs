namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public static class BuildingSyndicationExtensions
    {
        public static async Task<BuildingSyndicationItem> LatestPosition(
            this LegacyContext context,
            Guid buildingId,
            CancellationToken ct)
        {
            var result = context
                             .BuildingSyndication
                             .Local
                             .Where(x => x.BuildingId == buildingId)
                             .OrderByDescending(x => x.Position)
                             .FirstOrDefault()
                         ?? await context
                             .BuildingSyndication
                             .Where(x => x.BuildingId == buildingId)
                             .Include(x => x.BuildingUnits).ThenInclude(y => y.Addresses)
                             .Include(x => x.BuildingUnits).ThenInclude(y => y.Readdresses)
                             .OrderByDescending(x => x.Position)
                             .FirstOrDefaultAsync(ct);

            //if (result != null)
            //{
            //    await context.Entry(result).Collection(x => x.BuildingUnits).LoadAsync(ct);
            //    foreach (var buildingUnit in result.BuildingUnits)
            //    {
            //        await context.Entry(buildingUnit).Collection(x => x.Addresses).LoadAsync(ct);
            //        await context.Entry(buildingUnit).Collection(x => x.Readdresses).LoadAsync(ct);
            //    }
            //}

            return result;
        }
    }
}

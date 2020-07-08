namespace BuildingRegistry.Projections.Wfs.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingUnitProjectionsExtensions
    {
        public static async Task<IEnumerable<BuildingUnit>> GetByBuildingId(
            this DbSet<BuildingUnit> dbSet,
            Guid buildingId,
            CancellationToken cancellationToken)
        {
            return dbSet
                    .Local
                    .Where(x => x.BuildingId == buildingId)
                    .Union(await dbSet
                        .Where(x => x.BuildingId == buildingId)
                        .ToListAsync(cancellationToken));
        }
    }
}

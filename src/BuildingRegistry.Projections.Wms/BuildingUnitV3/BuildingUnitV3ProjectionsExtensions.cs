namespace BuildingRegistry.Projections.Wms.BuildingUnitV3
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingUnitProjectionsExtensions
    {
        public static async Task<IEnumerable<BuildingUnitV3>> GetByBuildingId(
            this DbSet<BuildingUnitV3> dbSet,
            int buildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            return dbSet
                    .Local
                    .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                    .Union(await dbSet
                        .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                        .ToListAsync(cancellationToken));
        }
    }
}

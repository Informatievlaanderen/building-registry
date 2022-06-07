namespace BuildingRegistry.Projections.Wms.BuildingUnitV2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingUnitProjectionsExtensions
    {
        public static async Task<IEnumerable<BuildingUnitV2>> GetByBuildingId(
            this DbSet<BuildingUnitV2> dbSet,
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

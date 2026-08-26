namespace BuildingRegistry.Projections.Wms
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;

        public DbSet<BuildingV3.BuildingV3> BuildingsV3 => Set<BuildingV3.BuildingV3>();
        public DbSet<BuildingUnitV2.BuildingUnitV2> BuildingUnitsV2 { get; set; }

        // The Lambert 2008 (EPSG 3812) counterparts of the two above, which stay Lambert 72. See ADR 0005.
        public DbSet<BuildingV4.BuildingV4> BuildingsV4 => Set<BuildingV4.BuildingV4>();
        public DbSet<BuildingUnitV3.BuildingUnitV3> BuildingUnitsV3 => Set<BuildingUnitV3.BuildingUnitV3>();

        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options)
        {
            if (!Database.IsInMemory())
            {
                Database.SetCommandTimeout(10 * 60);
            }
        }
    }
}

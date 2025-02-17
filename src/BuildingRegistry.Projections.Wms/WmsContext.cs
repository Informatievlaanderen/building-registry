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

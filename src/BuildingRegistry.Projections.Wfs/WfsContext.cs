namespace BuildingRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;
        public DbSet<BuildingV3.BuildingV3> BuildingsV3 => Set<BuildingV3.BuildingV3>();
        public DbSet<BuildingUnitV2.BuildingUnitV2> BuildingUnitsV2 { get; set; }

        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options)
        {
            if(!Database.IsInMemory())
            {
                Database.SetCommandTimeout(10 * 60);
            }
        }
    }
}

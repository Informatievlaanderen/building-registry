namespace BuildingRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;
        public DbSet<BuildingV3.BuildingV3> BuildingsV3 => Set<BuildingV3.BuildingV3>();
        public DbSet<BuildingUnitV2.BuildingUnitV2> BuildingUnitsV2 => Set<BuildingUnitV2.BuildingUnitV2>();
        public DbSet<BuildingUnitAddress.BuildingUnitAddress> BuildingUnitAddresses => Set<BuildingUnitAddress.BuildingUnitAddress>();

        // The Lambert 2008 (EPSG 3812) counterparts of BuildingsV3 and BuildingUnitsV2, which stay
        // Lambert 72. BuildingUnitAddresses carries no geometry, so both versions share it. See ADR 0005.
        public DbSet<BuildingV4.BuildingV4> BuildingsV4 => Set<BuildingV4.BuildingV4>();
        public DbSet<BuildingUnitV3.BuildingUnitV3> BuildingUnitsV3 => Set<BuildingUnitV3.BuildingUnitV3>();

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

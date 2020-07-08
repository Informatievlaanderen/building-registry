namespace BuildingRegistry.Projections.Wfs
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WfsContext : RunnerDbContext<WfsContext>
    {
        public override string ProjectionStateSchema => Schema.Wfs;

        public DbSet<Building.Building> Buildings { get; set; }
        public DbSet<BuildingUnit.BuildingUnit> BuildingUnits { get; set; }
        public DbSet<BuildingUnit.BuildingUnitBuildingItem> BuildingUnitsBuildings { get; set; }

        public WfsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WfsContext(DbContextOptions<WfsContext> options)
            : base(options)
        {
            Database.SetCommandTimeout(10 * 60);
        }
    }
}

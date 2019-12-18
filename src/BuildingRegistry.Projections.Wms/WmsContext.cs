namespace BuildingRegistry.Projections.Wms
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;

        public DbSet<Building.Building> Buildings { get; set; }
        public DbSet<BuildingUnit.BuildingUnit> BuildingUnits { get; set; }
        public DbSet<BuildingUnit.BuildingUnitBuildingPersistentLocalId> BuildingUnitBuildingPersistentLocalIds { get; set; }

        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options)
        {
            Database.SetCommandTimeout(5 * 60);
        }
    }
}

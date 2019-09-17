namespace BuildingRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingDetail;
    using BuildingSyndication;
    using BuildingUnitDetail;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<BuildingDetailItem> BuildingDetails { get; set; }
        public DbSet<BuildingSyndicationItem> BuildingSyndication { get; set; }
        public DbSet<BuildingUnitDetailItem> BuildingUnitDetails { get; set; }
        public DbSet<BuildingUnitBuildingItem> BuildingUnitBuildings { get; set; }
        public DbSet<BuildingUnitDetailAddressItem> BuildingUnitAddresses { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }
}

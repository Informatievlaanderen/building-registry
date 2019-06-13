namespace BuildingRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingDetail;
    using BuildingSyndication;
    using BuildingUnitDetail;
    using Microsoft.EntityFrameworkCore;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Infrastructure.Schema.Legacy;

        public DbSet<BuildingDetailItem> BuildingDetails { get; set; }
        public DbSet<BuildingSyndicationItem> BuildingSyndication { get; set; }
        public DbSet<BuildingUnitDetailItem> BuildingUnitDetails { get; set; }
        public DbSet<BuildingUnitBuildingOsloIdItem> BuildingUnitBuildingOsloIds { get; set; }
        public DbSet<BuildingUnitDetailAddressItem> BuildingUnitAddresses { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.BuildingRegistry.BuildingRegistryContext;Trusted_Connection=True;");
    }
}

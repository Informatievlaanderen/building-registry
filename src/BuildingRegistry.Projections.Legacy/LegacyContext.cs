namespace BuildingRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingDetail;
    using BuildingPersistentIdCrabIdMapping;
    using BuildingSyndication;
    using BuildingUnitDetail;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using PersistentLocalIdMigration;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string BuildingDetailListCountViewName = "vw_BuildingDetailListCountView";
        internal const string BuildingUnitDetailListCountViewName = "vw_BuildingUnitDetailListCountView";

        public DbSet<BuildingDetailItem> BuildingDetails { get; set; }
        public DbSet<BuildingSyndicationItem> BuildingSyndication { get; set; }
        public DbSet<BuildingUnitDetailItem> BuildingUnitDetails { get; set; }
        public DbSet<BuildingUnitBuildingItem> BuildingUnitBuildings { get; set; }
        public DbSet<BuildingUnitDetailAddressItem> BuildingUnitAddresses { get; set; }
        public DbSet<RemovedPersistentLocalId> RemovedPersistentLocalIds { get; set; }
        public DbSet<DuplicatedPersistentLocalId> DuplicatedPersistentLocalIds { get; set; }

        public DbSet<BuildingDetailListCountView> BuildingDetailListCountView { get; set; }
        public DbSet<BuildingUnitDetailListCountView> BuildingUnitDetailListCountView { get; set; }

        public DbSet<BuildingPersistentLocalIdCrabIdMapping> BuildingPersistentIdCrabIdMappings { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuildingDetailListCountView>()
                .HasNoKey()
                .ToView(BuildingDetailListCountViewName, Schema.Legacy);

            modelBuilder.Entity<BuildingUnitDetailListCountView>()
                .HasNoKey()
                .ToView(BuildingUnitDetailListCountViewName, Schema.Legacy);
        }
    }
}

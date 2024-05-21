namespace BuildingRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingDetail;
    using BuildingDetailV2;
    using BuildingPersistentIdCrabIdMapping;
    using BuildingSyndication;
    using BuildingUnitDetail;
    using BuildingUnitDetailV2;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using PersistentLocalIdMigration;
    using BuildingUnitDetailAddressItem = BuildingUnitDetail.BuildingUnitDetailAddressItem;
    using BuildingUnitDetailItem = BuildingUnitDetail.BuildingUnitDetailItem;
    using BuildingUnitDetailListCountView = BuildingUnitDetail.BuildingUnitDetailListCountView;
    using BuildingUnitDetailItemV2WithCount = BuildingUnitDetailV2WithCount.BuildingUnitDetailItemV2;
    using BuildingSyndicationItemWithCount = BuildingSyndicationWithCount.BuildingSyndicationItem;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string BuildingDetailListCountViewName = "vw_BuildingDetailListCountView";
        internal const string BuildingDetailV2ListCountViewName = "vw_BuildingDetailV2ListCountView";
        internal const string BuildingUnitDetailListCountViewName = "vw_BuildingUnitDetailListCountView";
        internal const string BuildingUnitDetailV2ListCountViewName = "vw_BuildingUnitDetailV2ListCountView";

        public DbSet<BuildingDetailItem> BuildingDetails { get; set; }
        public DbSet<BuildingDetailItemV2> BuildingDetailsV2 { get; set; }

        public DbSet<BuildingSyndicationItem> BuildingSyndication { get; set; }
        public DbSet<BuildingSyndicationItemWithCount> BuildingSyndicationWithCount { get; set; }

        public DbSet<BuildingUnitDetailItem> BuildingUnitDetails { get; set; }
        public DbSet<BuildingUnitBuildingItem> BuildingUnitBuildings { get; set; }
        public DbSet<BuildingUnitDetailAddressItem> BuildingUnitAddresses { get; set; }

        public DbSet<BuildingUnitDetailItemV2> BuildingUnitDetailsV2 { get; set; }
        public DbSet<BuildingUnitDetailItemV2WithCount> BuildingUnitDetailsV2WithCount { get; set; }

        public DbSet<RemovedPersistentLocalId> RemovedPersistentLocalIds { get; set; }
        public DbSet<DuplicatedPersistentLocalId> DuplicatedPersistentLocalIds { get; set; }

        public DbSet<BuildingDetailListCountView> BuildingDetailListCountView { get; set; }
        public DbSet<BuildingDetailV2ListCountView> BuildingDetailListCountViewV2 { get; set; }
        public DbSet<BuildingUnitDetailListCountView> BuildingUnitDetailListCountView { get; set; }
        public DbSet<BuildingUnitDetailV2ListCountView> BuildingUnitDetailListCountViewV2 { get; set; }

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

            modelBuilder.Entity<BuildingDetailV2ListCountView>()
                .HasNoKey()
                .ToView(BuildingDetailV2ListCountViewName, Schema.Legacy);

            modelBuilder.Entity<BuildingUnitDetailListCountView>()
                .HasNoKey()
                .ToView(BuildingUnitDetailListCountViewName, Schema.Legacy);

            modelBuilder.Entity<BuildingUnitDetailV2ListCountView>()
                .HasNoKey()
                .ToView(BuildingUnitDetailV2ListCountViewName, Schema.Legacy);
        }
    }
}

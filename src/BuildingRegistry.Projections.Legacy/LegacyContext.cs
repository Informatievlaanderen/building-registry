namespace BuildingRegistry.Projections.Legacy
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingDetailV2;
    using BuildingPersistentIdCrabIdMapping;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using PersistentLocalIdMigration;
    using BuildingUnitDetailItemV2WithCount = BuildingUnitDetailV2WithCount.BuildingUnitDetailItemV2;
    using BuildingSyndicationItemWithCount = BuildingSyndicationWithCount.BuildingSyndicationItem;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;
        internal const string BuildingDetailV2ListCountViewName = "vw_BuildingDetailV2ListCountView";
        internal const string BuildingUnitDetailV2ListCountViewName = "vw_BuildingUnitDetailV2ListCountView";

        public DbSet<BuildingDetailItemV2> BuildingDetailsV2 { get; set; }
        public DbSet<BuildingSyndicationItemWithCount> BuildingSyndicationWithCount { get; set; }
        public DbSet<BuildingUnitDetailItemV2WithCount> BuildingUnitDetailsV2WithCount { get; set; }

        public DbSet<RemovedPersistentLocalId> RemovedPersistentLocalIds { get; set; }
        public DbSet<DuplicatedPersistentLocalId> DuplicatedPersistentLocalIds { get; set; }

        public DbSet<BuildingDetailV2ListCountView> BuildingDetailListCountViewV2 { get; set; }
        public DbSet<BuildingUnitDetailV2ListCountView> BuildingUnitDetailListCountViewV2 { get; set; }

        public DbSet<BuildingPersistentLocalIdCrabIdMapping> BuildingPersistentIdCrabIdMappings { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }

        /// <summary>
        /// Whether any building is still missing its Lambert 2008 outline, which would make it invisible to
        /// matching done in that system. Backs <see cref="Lambert2008MatchingReadiness"/>.
        ///
        /// A building whose geometry is not a polygon has no <c>SysGeometry</c> either and is matchable in
        /// neither system, so it is not what this is asking about — counting it would leave the guard
        /// permanently tripped.
        ///
        /// Synchronous because the matching path that calls it is synchronous end to end; blocking on an
        /// asynchronous version from there would be sync-over-async for no gain.
        /// </summary>
        public bool HasIncompleteLambert2008Geometry()
            => BuildingDetailsV2
                .AsNoTracking()
                .Any(x => !x.IsRemoved && x.SysGeometry != null && x.SysGeometryLambert2008 == null);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuildingDetailV2ListCountView>()
                .HasNoKey()
                .ToView(BuildingDetailV2ListCountViewName, Schema.Legacy);

            modelBuilder.Entity<BuildingUnitDetailV2ListCountView>()
                .HasNoKey()
                .ToView(BuildingUnitDetailV2ListCountViewName, Schema.Legacy);
        }
    }
}

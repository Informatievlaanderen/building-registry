namespace BuildingRegistry.Projections.Feed
{
    using BuildingFeed;
    using BuildingUnitFeed;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class FeedContext : RunnerDbContext<FeedContext>
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public override string ProjectionStateSchema => Schema.Feed;

        public DbSet<BuildingFeedItem> BuildingFeed => Set<BuildingFeedItem>();

        public DbSet<BuildingDocument> BuildingDocuments => Set<BuildingDocument>();

        public DbSet<BuildingUnitFeedItem> BuildingUnitFeed => Set<BuildingUnitFeedItem>();

        public DbSet<BuildingUnitDocument> BuildingUnitDocuments => Set<BuildingUnitDocument>();

        public DbSet<BuildingGeometryForBuildingUnit> BuildingGeometryForBuildingUnit => Set<BuildingGeometryForBuildingUnit>();

        // This needs to be here to please EF
        public FeedContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public FeedContext(DbContextOptions<FeedContext> options, JsonSerializerSettings jsonSerializerSettings)
            : base(options)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("BuildingFeedSequence", Schema.Feed)
                .StartsAt(1)
                .IncrementsBy(1)
                .IsCyclic(false);

            modelBuilder.HasSequence<long>("BuildingUnitFeedSequence", Schema.Feed)
                .StartsAt(1)
                .IncrementsBy(1)
                .IsCyclic(false);

            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new BuildingFeedConfiguration());
            modelBuilder.ApplyConfiguration(new BuildingDocumentConfiguration(_jsonSerializerSettings));
            modelBuilder.ApplyConfiguration(new BuildingUnitFeedConfiguration());
            modelBuilder.ApplyConfiguration(new BuildingUnitDocumentConfiguration(_jsonSerializerSettings));
            modelBuilder.ApplyConfiguration(new BuildingGeometryForBuildingUnitConfiguration());
        }
    }
}

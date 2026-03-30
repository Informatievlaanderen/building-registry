namespace BuildingRegistry.Projections.Feed.BuildingFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingFeedItem
    {
        public long Id { get; set; }
        public int Page { get; set; }
        public long Position { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private BuildingFeedItem() { }

        public BuildingFeedItem(long position, int page) : this()
        {
            Page = page;
            Position = position;
        }
    }

    public class BuildingFeedItemBuilding
    {
        public long FeedItemId { get; set; }
        public int BuildingPersistentLocalId { get; set; }

        private BuildingFeedItemBuilding() { }

        public BuildingFeedItemBuilding(long feedItemId, int buildingPersistentLocalId) : this()
        {
            FeedItemId = feedItemId;
            BuildingPersistentLocalId = buildingPersistentLocalId;
        }
    }

    public class BuildingFeedConfiguration : IEntityTypeConfiguration<BuildingFeedItem>
    {
        private const string TableName = "BuildingFeed";

        public void Configure(EntityTypeBuilder<BuildingFeedItem> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.Id)
                .IsClustered();

            b.Property(x => x.Id)
                .UseHiLo("BuildingFeedSequence", Schema.Feed);

            b.Property(x => x.CloudEventAsString)
                .HasColumnName("CloudEvent")
                .IsRequired();

            b.Property(x => x.Application);
            b.Property(x => x.Modification);
            b.Property(x => x.Operator);
            b.Property(x => x.Organisation);
            b.Property(x => x.Reason);

            b.HasIndex(x => x.Position);
            b.HasIndex(x => x.Page);
        }
    }

    public class BuildingFeedItemBuildingConfiguration : IEntityTypeConfiguration<BuildingFeedItemBuilding>
    {
        private const string TableName = "BuildingFeedItemBuildings";

        public void Configure(EntityTypeBuilder<BuildingFeedItemBuilding> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => new { x.FeedItemId, x.BuildingPersistentLocalId });

            b.HasIndex(x => x.FeedItemId);
            b.HasIndex(x => x.BuildingPersistentLocalId);
        }
    }
}

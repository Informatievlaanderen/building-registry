namespace BuildingRegistry.Projections.Feed.BuildingUnitFeed
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitFeedItem
    {
        public long Id { get; set; }
        public int Page { get; set; }
        public long Position { get; set; }

        public int BuildingUnitPersistentLocalId { get; set; }

        public Application? Application { get; set; }
        public Modification? Modification { get; set; }
        public string? Operator { get; set; }
        public Organisation? Organisation { get; set; }
        public string? Reason { get; set; }
        public string CloudEventAsString { get; set; } = null!;

        private BuildingUnitFeedItem() { }

        public BuildingUnitFeedItem(long position, int page, int buildingUnitPersistentLocalId) : this()
        {
            Page = page;
            Position = position;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
        }
    }

    public class BuildingUnitFeedConfiguration : IEntityTypeConfiguration<BuildingUnitFeedItem>
    {
        private const string TableName = "BuildingUnitFeed";
        public const string SequenceName = "BuildingUnitFeedSequence";

        public void Configure(EntityTypeBuilder<BuildingUnitFeedItem> b)
        {
            b.ToTable(TableName, Schema.Feed)
                .HasKey(x => x.Id)
                .IsClustered();

            b.Property(x => x.Id)
                .UseHiLo(SequenceName, Schema.Feed);

            b.Property(x => x.BuildingUnitPersistentLocalId)
                .IsRequired();

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
            b.HasIndex(x => x.BuildingUnitPersistentLocalId);
        }
    }
}

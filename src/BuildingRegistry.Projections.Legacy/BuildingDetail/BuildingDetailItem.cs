namespace BuildingRegistry.Projections.Legacy.BuildingDetail
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;
    using GeoAPI.Geometries;
    using ValueObjects;

    public class BuildingDetailItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid BuildingId { get; set; }
        public int? OsloId { get; set; }
        public BuildingGeometryMethod? GeometryMethod { get; set; }
        public IPolygon Geometry { get; set; }
        public BuildingStatus? Status { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class BuildingDetailItemConfiguration : IEntityTypeConfiguration<BuildingDetailItem>
    {
        public const string TableName = "BuildingDetails";

        public void Configure(EntityTypeBuilder<BuildingDetailItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingId)
                .ForSqlServerIsClustered(false);

            b.Property(BuildingDetailItem.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Ignore(x => x.Version);

            b.Property(p => p.BuildingId);
            b.Property(p => p.GeometryMethod);
            b.Property(p => p.Geometry)
                .HasColumnType("sys.geometry");
            b.Property(p => p.Status);
            b.Property(p => p.IsComplete);
            b.Property(p => p.IsRemoved);

            b.HasIndex(p => p.OsloId).IsUnique();
        }
    }
}

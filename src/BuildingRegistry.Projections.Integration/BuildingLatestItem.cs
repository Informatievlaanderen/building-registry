namespace BuildingRegistry.Projections.Integration
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using BuildingRegistry.Infrastructure;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public sealed class BuildingLatestItem
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int BuildingPersistentLocalId { get; set; }
        public string Status { get; set; }
        public string GeometryMethod { get; set; }
        public Geometry Geometry { get; set; }
        public string? NisCode { get; set; }
        public bool IsRemoved { get; set; }

        public string PuriId { get; set; }
        public string Namespace { get; set; }
        public string VersionAsString { get; set; }
        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant VersionTimestamp
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public long IdempotenceKey { get; set; }

        public BuildingLatestItem()
        { }
    }

    public sealed class BuildingLatestItemConfiguration : IEntityTypeConfiguration<BuildingLatestItem>
    {
        public void Configure(EntityTypeBuilder<BuildingLatestItem> builder)
        {
            const string tableName = "building_latest_items";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => x.BuildingPersistentLocalId);

            builder.Property(x => x.BuildingPersistentLocalId).HasColumnName("building_persistent_local_id");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.GeometryMethod).HasColumnName("geometry_method");
            builder.Property(x => x.Geometry).HasColumnName("geometry");
            builder.Property(x => x.NisCode).HasColumnName("nis_code");
            builder.Property(x => x.IsRemoved).HasColumnName("is_removed");
            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(BuildingLatestItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
            builder.Property(x => x.IdempotenceKey).HasColumnName("idempotence_key");

            builder.Ignore(x => x.VersionTimestamp);

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.IsRemoved);
            builder.HasIndex(x => x.NisCode);
        }
    }
}

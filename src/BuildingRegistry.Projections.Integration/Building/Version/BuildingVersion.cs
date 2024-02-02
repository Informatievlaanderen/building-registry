namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public sealed class BuildingVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

        public long Position { get; set; }

        public Guid? BuildingId { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        public string? Status { get; set; }
        public string? OsloStatus { get; set; }
        public string? GeometryMethod { get; set; }
        public string? OsloGeometryMethod { get; set; }
        public Geometry? Geometry { get; set; }
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

        public string CreatedOnAsString { get; set; }
        private DateTimeOffset CreatedOnTimestampAsDateTimeOffset { get; set; }

        public Instant CreatedOnTimestamp
        {
            get => Instant.FromDateTimeOffset(CreatedOnTimestampAsDateTimeOffset);
            set
            {
                CreatedOnTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                CreatedOnAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public BuildingVersion()
        { }

        public BuildingVersion CloneAndApplyEventInfo(
            long newPosition,
            Instant lastChangedOn,
            Action<BuildingVersion> editFunc)
        {
            var newItem = new BuildingVersion
            {
                Position = newPosition,

                BuildingId = BuildingId,
                BuildingPersistentLocalId = BuildingPersistentLocalId,
                Status = Status,
                OsloStatus = OsloStatus,
                GeometryMethod = GeometryMethod,
                OsloGeometryMethod = OsloGeometryMethod,
                Geometry = Geometry,
                IsRemoved = IsRemoved,

                PuriId = PuriId,
                Namespace = Namespace,

                VersionTimestamp = lastChangedOn,
                CreatedOnTimestamp = CreatedOnTimestamp
            };

            editFunc(newItem);

            return newItem;
        }
    }

    public sealed class BuildingLatestEventConfiguration : IEntityTypeConfiguration<BuildingVersion>
    {
        public void Configure(EntityTypeBuilder<BuildingVersion> builder)
        {
            const string tableName = "building_versions";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => x.Position);

            builder.Property(x => x.Position).ValueGeneratedNever();

            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.BuildingId).HasColumnName("building_id");
            builder.Property(x => x.BuildingPersistentLocalId).HasColumnName("building_persistent_local_id");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.OsloStatus).HasColumnName("oslo_status");
            builder.Property(x => x.GeometryMethod).HasColumnName("geometry_method");
            builder.Property(x => x.OsloGeometryMethod).HasColumnName("oslo_geometry_method");
            builder.Property(x => x.Geometry).HasColumnName("geometry");
            builder.Property(x => x.IsRemoved).HasColumnName("is_removed");
            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(BuildingVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
            builder.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
            builder.Property(BuildingVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

            builder.Ignore(x => x.VersionTimestamp);
            builder.Ignore(x => x.CreatedOnTimestamp);

            builder.HasIndex(x => x.BuildingPersistentLocalId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.OsloStatus);
            builder.HasIndex(x => x.IsRemoved);
            builder.HasIndex(x => x.Geometry).HasMethod("GIST");
            builder.HasIndex(BuildingVersion.VersionTimestampBackingPropertyName);
        }
    }
}

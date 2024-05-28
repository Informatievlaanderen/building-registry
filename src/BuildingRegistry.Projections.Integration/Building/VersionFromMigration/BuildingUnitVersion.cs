namespace BuildingRegistry.Projections.Integration.Building.VersionFromMigration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public sealed class BuildingUnitVersion
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
        public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

        public long Position { get; set; }

        public int BuildingUnitPersistentLocalId { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        public string Status { get; set; }
        public string OsloStatus { get; set; }
        public string Function { get; set; }
        public string OsloFunction { get; set; }
        public string Type { get; set; }
        public string GeometryMethod { get; set; }
        public string OsloGeometryMethod { get; set; }
        public Geometry Geometry { get; set; }
        public bool HasDeviation { get; set; }
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

        public Collection<BuildingUnitAddressVersion> Addresses { get; set; }

        public BuildingUnitVersion()
        {
            Addresses = new Collection<BuildingUnitAddressVersion>();
        }

        public BuildingUnitVersion CloneAndApplyEventInfo(long newPosition, string eventName)
        {
            var newItem = new BuildingUnitVersion
            {
                Position = newPosition,

                BuildingUnitPersistentLocalId = BuildingUnitPersistentLocalId,
                BuildingPersistentLocalId = BuildingPersistentLocalId,
                Status = Status,
                OsloStatus = OsloStatus,
                Type = eventName,
                Function = Function,
                OsloFunction = OsloFunction,
                GeometryMethod = GeometryMethod,
                OsloGeometryMethod = OsloGeometryMethod,
                Geometry = Geometry,
                HasDeviation = HasDeviation,
                IsRemoved = IsRemoved,

                PuriId = PuriId,
                Namespace = Namespace,

                VersionTimestamp = VersionTimestamp,
                CreatedOnTimestamp = CreatedOnTimestamp,

                Addresses = new Collection<BuildingUnitAddressVersion>(
                    Addresses.Select(x => x.CloneAndApplyEventInfo(newPosition)).ToList())
            };

            return newItem;
        }
    }

    public sealed class BuildingUnitVersionConfiguration : IEntityTypeConfiguration<BuildingUnitVersion>
    {
        public void Configure(EntityTypeBuilder<BuildingUnitVersion> builder)
        {
            const string tableName = "building_unit_versions_migration";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new { x.Position, x.BuildingUnitPersistentLocalId });

            builder.Property(x => x.Position).ValueGeneratedNever();

            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.BuildingUnitPersistentLocalId).HasColumnName("building_unit_persistent_local_id");
            builder.Property(x => x.BuildingPersistentLocalId).HasColumnName("building_persistent_local_id");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.OsloStatus).HasColumnName("oslo_status");
            builder.Property(x => x.Type).HasColumnName("type");
            builder.Property(x => x.Function).HasColumnName("function");
            builder.Property(x => x.OsloFunction).HasColumnName("oslo_function");
            builder.Property(x => x.GeometryMethod).HasColumnName("geometry_method");
            builder.Property(x => x.OsloGeometryMethod).HasColumnName("oslo_geometry_method");
            builder.Property(x => x.Geometry).HasColumnName("geometry");
            builder.Property(x => x.HasDeviation).HasColumnName("has_deviation");
            builder.Property(x => x.IsRemoved).HasColumnName("is_removed");
            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(BuildingUnitVersion.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");
            builder.Property(x => x.CreatedOnAsString).HasColumnName("created_on_as_string");
            builder.Property(BuildingUnitVersion.CreatedOnTimestampBackingPropertyName).HasColumnName("created_on_timestamp");

            builder.HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => new { x.Position, x.BuildingUnitPersistentLocalId });

            builder.Ignore(x => x.VersionTimestamp);
            builder.Ignore(x => x.CreatedOnTimestamp);

            builder.HasIndex(x => x.BuildingUnitPersistentLocalId);
            builder.HasIndex(x => x.BuildingPersistentLocalId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.OsloStatus);
            builder.HasIndex(x => x.Type);
            builder.HasIndex(x => x.IsRemoved);
            builder.HasIndex(x => x.Geometry).HasMethod("GIST");
            builder.HasIndex(BuildingUnitVersion.VersionTimestampBackingPropertyName);
        }
    }
}

﻿namespace BuildingRegistry.Projections.Integration
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

        public long Position { get; set; }

        public Guid? BuildingId { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        public string? Status { get; set; }
        public string? GeometryMethod { get; set; }
        public string? GeometryGml { get; set; }
        public Geometry? Geometry { get; set; }
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

                PuriId = PuriId,
                Namespace = Namespace,

                VersionTimestamp = lastChangedOn
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

            builder.Property(x => x.BuildingId).HasColumnName("building_id");
            builder.Property(x => x.BuildingPersistentLocalId).HasColumnName("building_persistent_local_id");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.GeometryMethod).HasColumnName("geometry_method");
            builder.Property(x => x.GeometryGml).HasColumnName("geometry_gml");
            builder.Property(x => x.Geometry).HasColumnName("geometry");
            builder.Property(x => x.NisCode).HasColumnName("nis_code");
            builder.Property(x => x.IsRemoved).HasColumnName("is_removed");
            builder.Property(x => x.PuriId).HasColumnName("puri_id");
            builder.Property(x => x.Namespace).HasColumnName("namespace");
            builder.Property(x => x.VersionAsString).HasColumnName("version_as_string");
            builder.Property(BuildingLatestItem.VersionTimestampBackingPropertyName).HasColumnName("version_timestamp");

            builder.Ignore(x => x.VersionTimestamp);

            builder.HasIndex(x => x.BuildingPersistentLocalId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.IsRemoved);
            builder.HasIndex(x => x.NisCode);
        }
    }
}

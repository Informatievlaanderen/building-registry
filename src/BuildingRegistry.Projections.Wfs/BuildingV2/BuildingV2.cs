namespace BuildingRegistry.Projections.Wfs.BuildingV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class BuildingV2
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public string Id { get; set; }

        public Geometry? Geometry { get; set; }
        public string? GeometryMethod { get; set; }
        public bool IsRemoved { get; set; }

        public string Status { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set
            {
                VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
                VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
            }
        }

        public string VersionAsString { get; protected set; }
    }

    public class BuildingConfiguration : IEntityTypeConfiguration<BuildingV2>
    {
        public const string TableName = "BuildingsV2";

        public void Configure(EntityTypeBuilder<BuildingV2> b)
        {
            b.ToTable(TableName, Schema.Wfs)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();

            b.Property(p => p.PersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.Id);

            b.Property(BuildingV2.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Property(p => p.VersionAsString);
            b.Ignore(x => x.Version);

            b.Property(p => p.GeometryMethod);

            b.Property(p => p.Geometry)
                .HasColumnType("sys.geometry");

            b.Property(p => p.Status);

            b.HasIndex(p => p.PersistentLocalId);
            b.HasIndex(p => p.Id);
            b.HasIndex(p => p.VersionAsString);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.GeometryMethod);

            b.HasIndex(p => p.IsRemoved);
        }
    }
}

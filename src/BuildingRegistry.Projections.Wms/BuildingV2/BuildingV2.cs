namespace BuildingRegistry.Projections.Wms.BuildingV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using BuildingRegistry.Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingV2
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public string Id { get; set; }

        public byte[]? Geometry { get; set; }
        public string GeometryMethod { get; set; }

        public BuildingStatus Status { get; set; }

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

        public string? VersionAsString { get; protected set; }
    }

    public class BuildingConfiguration : IEntityTypeConfiguration<BuildingV2>
    {
        public const string TableName = "BuildingsV2";

        public void Configure(EntityTypeBuilder<BuildingV2> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();

            b.Property(p => p.PersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.Id)
                .HasColumnType("varchar(46)")
                .HasMaxLength(46);

            b.Property(BuildingV2.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");
            b.Property(p => p.VersionAsString);

            b.Ignore(x => x.Version);

            b.Property(p => p.GeometryMethod)
                .HasColumnType("varchar(12)")
                .HasMaxLength(12);

            b.Property(p => p.Status)
                .HasConversion(x => x.Value, y => BuildingStatus.Parse(y));

            b.Property(p => p.Geometry);

            b.HasIndex(p => p.Status);
        }
    }
}

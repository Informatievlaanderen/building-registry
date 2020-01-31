namespace BuildingRegistry.Projections.Wms.Building
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ValueObjects;

    public class Building
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid BuildingId { get; set; }
        public int? PersistentLocalId { get; set; }
        public string? Id { get; set; }

        public byte[]? Geometry { get; set; }
        public string? GeometryMethod { get; set; }
        public bool IsComplete { get; set; }

        public BuildingStatus? Status
        {
            get => string.IsNullOrEmpty(StatusAsText) ? null : (BuildingStatus?)Enum.Parse(typeof(BuildingStatus), StatusAsText, true);
            set => StatusAsText = value.HasValue ? value.ToString() : null;
        }

        public string? StatusAsText { get; set; }
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

    public class BuildingConfiguration : IEntityTypeConfiguration<Building>
    {
        public const string TableName = "Buildings";

        public void Configure(EntityTypeBuilder<Building> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.BuildingId)
                .IsClustered(true);

            b.Property(p => p.Id)
                .HasColumnType("varchar(46)")
                .HasMaxLength(46);

            b.Property(Building.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");
            b.Property(p => p.VersionAsString);

            b.Ignore(x => x.Version);

            b.Property(p => p.GeometryMethod)
                .HasColumnType("varchar(12)")
                .HasMaxLength(12);

            b.Property(p => p.StatusAsText)
                .HasColumnName("Status");

            b.Property(p => p.Geometry);

            b.Ignore(p => p.Status);
            b.HasIndex(p => p.StatusAsText);
        }
    }
}

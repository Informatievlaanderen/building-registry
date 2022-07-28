namespace BuildingRegistry.Projections.Wms.BuildingUnit
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure;
    using Legacy;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingUnit
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid BuildingUnitId { get; set; }
        public string? Id { get; set; }
        public int? BuildingUnitPersistentLocalId { get; set; }

        public Guid BuildingId { get; set; }
        public int? BuildingPersistentLocalId { get; set; }

        public byte[]? Position { get; set; }
        public string? PositionMethod { get; set; }
        public string? Function { get; set; }
        public bool IsComplete { get; set; }
        public bool IsBuildingComplete { get; set; }

        public BuildingUnitStatus? Status
        {
            get => BuildingUnitStatus.Parse(StatusAsText);
            set => StatusAsText = value?.Status;
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

    public class BuildingUnitConfiguration : IEntityTypeConfiguration<BuildingUnit>
    {
        public const string TableName = "BuildingUnits";

        public void Configure(EntityTypeBuilder<BuildingUnit> b)
        {
            b.ToTable(TableName, Schema.Wms)
                .HasKey(p => p.BuildingUnitId)
                .IsClustered(true);

            b.Property(p => p.Id)
                .HasColumnType("varchar(53)")
                .HasMaxLength(53);

            b.Property(BuildingUnit.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");
            b.Property(p => p.VersionAsString);

            b.Ignore(x => x.Version);

            b.Property(p => p.BuildingId);
            b.Property(p => p.BuildingPersistentLocalId);
            b.Property(p => p.IsComplete);
            b.Property(p => p.IsBuildingComplete);
            b.Property(p => p.Function)
                .HasColumnType("varchar(21)")
                .HasMaxLength(21);

            b.Property(p => p.PositionMethod)
                .HasColumnType("varchar(22)")
                .HasMaxLength(22);

            b.Property(p => p.Position);

            b.Property(p => p.StatusAsText)
                .HasColumnName("Status");

            b.Ignore(p => p.Status);

            b.HasIndex(p => p.BuildingId);
            b.HasIndex(p => p.StatusAsText);
            b.HasIndex(p => new { p.IsComplete, p.IsBuildingComplete });
        }
    }
}

namespace BuildingRegistry.Projections.Wfs.BuildingUnit
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class BuildingUnit
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid BuildingUnitId { get; set; }
        public string? Id { get; set; }
        public int? BuildingUnitPersistentLocalId { get; set; }

        public Guid BuildingId { get; set; }
        public int? BuildingPersistentLocalId { get; set; }

        public Geometry? Position { get; set; }
        public string? PositionMethod { get; set; }
        public string? Function { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsBuildingComplete { get; set; }

        public string? Status { get; set; }
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
            b.ToTable(TableName, Schema.Wfs)
                .HasKey(p => p.BuildingUnitId)
                .IsClustered();

            b.Property(p => p.Id);

            b.Property(BuildingUnit.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Property(p => p.VersionAsString);
            b.Ignore(x => x.Version);

            b.Property(p => p.BuildingId);
            b.Property(p => p.BuildingPersistentLocalId);
            b.Property(p => p.IsComplete);
            b.Property(p => p.IsBuildingComplete);
            b.Property(p => p.Function);

            b.Property(p => p.PositionMethod);

            b.Property(p => p.Position)
                .HasColumnType("sys.geometry");

            b.Property(p => p.Status);

            b.HasIndex(p => p.BuildingId);

            b.HasIndex(p => p.Id);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.BuildingUnitPersistentLocalId);
            b.HasIndex(p => p.BuildingPersistentLocalId);
            b.HasIndex(p => p.PositionMethod);
            b.HasIndex(p => p.VersionAsString);
            b.HasIndex(p => p.Function);

            b.HasIndex(p => new { p.IsComplete, p.IsRemoved, p.IsBuildingComplete });
        }
    }
}

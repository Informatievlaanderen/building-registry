namespace BuildingRegistry.Projections.Wfs.BuildingUnitV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class BuildingUnitV2
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int BuildingUnitPersistentLocalId { get; set; }
        public string Id { get; set; }

        public int BuildingPersistentLocalId { get; set; }

        public Geometry? Position { get; set; }
        public string? PositionMethod { get; set; }
        public string Function { get; set; }
        public bool IsRemoved { get; set; }

        public string Status { get; set; }
        public bool HasDeviation { get; set; }
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

    public class BuildingUnitConfiguration : IEntityTypeConfiguration<BuildingUnitV2>
    {
        public const string TableName = "BuildingUnitsV2";

        public void Configure(EntityTypeBuilder<BuildingUnitV2> b)
        {
            b.ToTable(TableName, Schema.Wfs)
                .HasKey(p => p.BuildingUnitPersistentLocalId)
                .IsClustered();

            b.Property(p => p.BuildingUnitPersistentLocalId)
                .ValueGeneratedNever();

            b.Property(p => p.Id);

            b.Property(BuildingUnitV2.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Property(p => p.VersionAsString);
            b.Ignore(x => x.Version);

            b.Property(p => p.BuildingPersistentLocalId);
            b.Property(p => p.Function);

            b.Property(p => p.PositionMethod);

            b.Property(p => p.Position)
                .HasColumnType("sys.geometry");

            b.Property(p => p.Status);
            b.Property(p => p.HasDeviation)
                .HasDefaultValue(false);

            b.HasIndex(p => p.Id);
            b.HasIndex(p => p.Status);
            b.HasIndex(p => p.BuildingUnitPersistentLocalId);
            b.HasIndex(p => p.BuildingPersistentLocalId);
            b.HasIndex(p => p.PositionMethod);
            b.HasIndex(p => p.VersionAsString);
            b.HasIndex(p => p.Function);
            b.HasIndex(p => p.IsRemoved);
        }
    }
}

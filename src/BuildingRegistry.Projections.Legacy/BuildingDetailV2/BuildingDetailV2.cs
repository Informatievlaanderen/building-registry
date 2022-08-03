namespace BuildingRegistry.Projections.Legacy.BuildingDetailV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingDetailItemV2
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public BuildingGeometryMethod GeometryMethod { get; set; }
        public byte[] Geometry { get; set; }
        public BuildingStatus Status { get; set; }
        public bool IsRemoved { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public string? LastEventHash { get; set; }

        private BuildingDetailItemV2()
        {
            Geometry = Array.Empty<byte>();
        }

        public BuildingDetailItemV2(
            int persistentLocalId,
            BuildingGeometryMethod buildingGeometryMethod,
            byte[] geometry,
            BuildingStatus status,
            bool isRemoved,
            Instant version)
        {
            PersistentLocalId = persistentLocalId;
            GeometryMethod = buildingGeometryMethod;
            Geometry = geometry;
            Status = status;
            IsRemoved = isRemoved;
            Version = version;
        }

        internal void SetGeometry(string extendedWkb, BuildingGeometryMethod geometryMethod)
        {
            Geometry = extendedWkb.ToByteArray();
            GeometryMethod = geometryMethod;
        }
    }

    public class BuildingDetailItemConfiguration : IEntityTypeConfiguration<BuildingDetailItemV2>
    {
        internal const string TableName = "BuildingDetailsV2";

        public void Configure(EntityTypeBuilder<BuildingDetailItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();

            b.Property(BuildingDetailItemV2.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Ignore(x => x.Version);

            b.Property(p => p.PersistentLocalId)
                .ValueGeneratedNever();
            b.Property(p => p.GeometryMethod)
                .HasConversion(x => x.Value, y => BuildingGeometryMethod.Parse(y));
            b.Property(p => p.Geometry);
            b.Property(p => p.Status)
                .HasConversion(x => x.Value, y => BuildingStatus.Parse(y));
            b.Property(p => p.IsRemoved);

            b.HasIndex(p => new { p.IsRemoved, p.PersistentLocalId });
            b.HasIndex(p => p.Status);
        }
    }
}

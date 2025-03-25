namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class BuildingDetail
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public BuildingGeometryMethod GeometryMethod { get; set; }
        public byte[] Geometry { get; set; }

        public BuildingStatus Status
        {
            get => BuildingStatus.Parse(StatusAsString);
            set => StatusAsString = value.Value;
        }
        public string StatusAsString { get; private set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }
        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        private BuildingDetail()
        {
            Geometry = [];
        }

        public BuildingDetail(
            int persistentLocalId,
            BuildingGeometryMethod buildingGeometryMethod,
            byte[] geometry,
            Geometry sysGeometry,
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

    public class BuildingDetailConfiguration : IEntityTypeConfiguration<BuildingDetail>
    {
        internal const string TableName = "Buildings";

        public void Configure(EntityTypeBuilder<BuildingDetail> b)
        {
            b.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();
            b.Property(p => p.PersistentLocalId).ValueGeneratedNever();

            b.Property(BuildingDetail.VersionTimestampBackingPropertyName).HasColumnName("Version");
            b.Ignore(x => x.Version);

            b.Property(p => p.GeometryMethod).HasConversion(x => x.Value, y => BuildingGeometryMethod.Parse(y));
            b.Property(p => p.Geometry);

            b.Property(x => x.StatusAsString).HasColumnName("Status");
            b.Ignore(p => p.Status);

            b.Property(p => p.IsRemoved);

            b.HasIndex(x => x.IsRemoved);
        }
    }
}

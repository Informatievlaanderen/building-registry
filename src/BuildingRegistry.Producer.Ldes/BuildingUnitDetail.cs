namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingUnitDetail
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int BuildingUnitPersistentLocalId { get; set; }
        public int BuildingPersistentLocalId { get; set; }

        public byte[] Position { get; set; }
        public BuildingUnitPositionGeometryMethod PositionMethod { get; set; }

        public BuildingUnitFunction Function
        {
            get => BuildingUnitFunction.Parse(FunctionAsString);
            set => FunctionAsString = value.Function;
        }
        public string FunctionAsString { get; private set; }

        public BuildingUnitStatus Status
        {
            get => BuildingUnitStatus.Parse(StatusAsString);
            set => StatusAsString = value.Status;
        }
        public string StatusAsString { get; private set; }

        public bool HasDeviation { get; set; }

        public virtual ICollection<BuildingUnitDetailAddress> Addresses { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        private BuildingUnitDetail()
        {
            Addresses = [];
            Position = [];
        }

        public BuildingUnitDetail(
            int buildingUnitPersistentLocalId,
            int buildingPersistentLocalId,
            byte[] position,
            BuildingUnitPositionGeometryMethod positionMethod,
            BuildingUnitFunction function,
            BuildingUnitStatus status,
            bool hasDeviation,
            ICollection<BuildingUnitDetailAddress> addresses,
            bool isRemoved,
            Instant version)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Position = position;
            PositionMethod = positionMethod;
            Function = function;
            Status = status;
            HasDeviation = hasDeviation;
            Addresses = addresses;
            IsRemoved = isRemoved;
            Version = version;
        }

        internal void SetGeometry(string extendedWkb, BuildingUnitPositionGeometryMethod geometryMethod)
        {
            Position = extendedWkb.ToByteArray();
            PositionMethod = geometryMethod;
        }
    }

    public class BuildingUnitDetailConfiguration : IEntityTypeConfiguration<BuildingUnitDetail>
    {
        internal const string TableName = "BuildingUnits";

        public void Configure(EntityTypeBuilder<BuildingUnitDetail> b)
        {
            b.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => p.BuildingUnitPersistentLocalId)
                .IsClustered();

            b.Property(p => p.BuildingUnitPersistentLocalId).ValueGeneratedNever();

            b.Property(BuildingUnitDetail.VersionTimestampBackingPropertyName).HasColumnName("Version");
            b.Ignore(x => x.Version);

            b.Property(p => p.HasDeviation).HasDefaultValue(false);

            b.Property(p => p.Position);
            b.Property(p => p.PositionMethod)
                .HasConversion(x => x.GeometryMethod, y => BuildingUnitPositionGeometryMethod.Parse(y));

            b.HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => x.BuildingUnitPersistentLocalId);

            b.Property(x => x.StatusAsString).HasColumnName("Status");
            b.Ignore(p => p.Status);

            b.Property(x => x.FunctionAsString).HasColumnName("Function");
            b.Ignore(p => p.Function);

            b.Property(p => p.IsRemoved);

            b.HasIndex(x => x.BuildingPersistentLocalId);
            b.HasIndex(x => x.IsRemoved);
        }
    }
}

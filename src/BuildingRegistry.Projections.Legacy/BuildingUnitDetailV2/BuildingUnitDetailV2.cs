namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2
{
    using System;
    using System.Collections.ObjectModel;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingUnitDetailItemV2
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

        public virtual Collection<BuildingUnitDetailAddressItemV2> Addresses { get; set; }

        public bool IsRemoved { get; set; }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }
        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public string? LastEventHash { get; set; }

        private BuildingUnitDetailItemV2()
        {
            Addresses = new Collection<BuildingUnitDetailAddressItemV2>();
            Position = Array.Empty<byte>();
        }

        public BuildingUnitDetailItemV2(
            int buildingUnitPersistentLocalId,
            int buildingPersistentLocalId,
            byte[] position,
            BuildingUnitPositionGeometryMethod positionMethod,
            BuildingUnitFunction function,
            BuildingUnitStatus status,
            bool hasDeviation,
            Collection<BuildingUnitDetailAddressItemV2> addresses,
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

    public class BuildingUnitDetailItemConfiguration : IEntityTypeConfiguration<BuildingUnitDetailItemV2>
    {
        internal const string TableName = "BuildingUnitDetailsV2";

        public void Configure(EntityTypeBuilder<BuildingUnitDetailItemV2> b)
        {
           b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingUnitPersistentLocalId)
                .IsClustered();

           b.Property(p => p.BuildingUnitPersistentLocalId)
               .ValueGeneratedNever();

            b.Property(p => p.Position);

            b.Ignore(x => x.Version);
            b.Property(BuildingUnitDetailItemV2.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Property(p => p.HasDeviation)
                .HasDefaultValue(false);

            b.Property(p => p.PositionMethod)
                .HasConversion(x => x.GeometryMethod, y => BuildingUnitPositionGeometryMethod.Parse(y));

            b.Property(p => p.IsRemoved);

            b.HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => x.BuildingUnitPersistentLocalId);

            b.HasIndex(p => p.BuildingPersistentLocalId);
            b.HasIndex(p => new { p.IsRemoved, p.BuildingUnitPersistentLocalId, p.BuildingPersistentLocalId });

            b.Ignore(p => p.Status);
            b.Property(x => x.StatusAsString).HasMaxLength(450)
                .HasColumnName("Status");
            b.HasIndex(x => x.StatusAsString);

            b.Ignore(p => p.Function);
            b.Property(x => x.FunctionAsString)
                .HasColumnName("Function");
            b.HasIndex(x => x.FunctionAsString);
        }
    }
}

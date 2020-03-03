namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetail
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;
    using System.Collections.ObjectModel;
    using ValueObjects;

    public class BuildingUnitDetailItem
    {
        public static string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public Guid BuildingUnitId { get; set; }
        public Guid BuildingId { get; set; }
        public int? PersistentLocalId { get; set; }
        public int? BuildingPersistentLocalId { get; set; }
        public byte[]? Position { get; set; }
        public bool IsComplete { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsBuildingComplete { get; set; }

        public BuildingUnitFunction? Function
        {
            get => BuildingUnitFunction.Parse(FunctionAsString);
            set => FunctionAsString = value.HasValue ? value.Value.Function : string.Empty;
        }

        public string? FunctionAsString { get; set; }

        public BuildingUnitPositionGeometryMethod? PositionMethod
        {
            get => string.IsNullOrEmpty(PositionMethodAsString) ? null : (BuildingUnitPositionGeometryMethod?)BuildingUnitPositionGeometryMethod.Parse(PositionMethodAsString);
            set => PositionMethodAsString = value?.GeometryMethod;
        }
        public string? PositionMethodAsString { get; set; }

        public BuildingUnitStatus? Status
        {
            get => BuildingUnitStatus.Parse(StatusAsString);
            set => StatusAsString = value.HasValue ? value.Value.Status : string.Empty;
        }
        public string? StatusAsString { get; set; }

        public virtual Collection<BuildingUnitDetailAddressItem> Addresses { get; set; }

        public BuildingUnitDetailItem()
        {
            Addresses = new Collection<BuildingUnitDetailAddressItem>();
        }

        private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }
    }

    public class BuildingUnitDetailItemConfiguration : IEntityTypeConfiguration<BuildingUnitDetailItem>
    {
        internal const string TableName = "BuildingUnitDetails";

        public void Configure(EntityTypeBuilder<BuildingUnitDetailItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.BuildingUnitId)
                .IsClustered(false);

            b.Property(p => p.BuildingId);
            b.Property(p => p.Position);

            b.Property(BuildingUnitDetailItem.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Ignore(x => x.Version);

            b.Ignore(p => p.Status);
            b.Property(p => p.StatusAsString)
                 .HasColumnName("Status");

            b.Ignore(p => p.PositionMethod);
            b.Property(p => p.PositionMethodAsString)
                .HasColumnName("PositionMethod");

            b.Ignore(p => p.Function);
            b.Property(p => p.FunctionAsString)
                .HasColumnName("Function");

            b.Property(p => p.IsComplete);
            b.Property(p => p.IsRemoved);
            b.Property(p => p.IsBuildingComplete);

            b.HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => x.BuildingUnitId);

            b.HasIndex(p => p.BuildingId);
            b.HasIndex(p => p.PersistentLocalId)
                .IsClustered();

            b.HasIndex(p => p.BuildingPersistentLocalId);
            b.HasIndex(p => new { p.IsComplete, p.IsRemoved, p.PersistentLocalId, p.IsBuildingComplete, p.BuildingPersistentLocalId });
        }
    }
}

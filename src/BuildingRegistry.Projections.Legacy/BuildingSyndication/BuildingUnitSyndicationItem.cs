namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NodaTime;
    using ValueObjects;

    public class BuildingUnitSyndicationItem
    {
        public long Position { get; set; }
        public Guid BuildingUnitId { get; set; }
        public int? OsloId { get; set; }
        public string PositionWkbHex { get; set; }
        public bool IsComplete { get; set; }

        public BuildingUnitFunction? Function
        {
            get => BuildingUnitFunction.Parse(FunctionAsString);
            set => FunctionAsString = value.HasValue ? value.Value.Function : string.Empty;
        }

        public string FunctionAsString { get; set; }

        public BuildingUnitPositionGeometryMethod? PositionMethod
        {
            get => string.IsNullOrEmpty(PositionMethodAsString) ? null : (BuildingUnitPositionGeometryMethod?)BuildingUnitPositionGeometryMethod.Parse(PositionMethodAsString);
            set => PositionMethodAsString = value?.GeometryMethod;
        }
        public string PositionMethodAsString { get; set; }

        public BuildingUnitStatus? Status
        {
            get => BuildingUnitStatus.Parse(StatusAsString);
            set => StatusAsString = value.HasValue ? value.Value.Status : string.Empty;
        }
        public string StatusAsString { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Collection<BuildingUnitAddressSyndicationItem> Addresses { get; set; }
        public Collection<BuildingUnitReaddressSyndicationItem> Readdresses { get; set; }

        public BuildingUnitSyndicationItem()
        {
            Addresses = new Collection<BuildingUnitAddressSyndicationItem>();
            Readdresses = new Collection<BuildingUnitReaddressSyndicationItem>();
        }

        public BuildingUnitSyndicationItem CloneAndApplyEventInfo(long position)
        {
            var newItem = new BuildingUnitSyndicationItem
            {
                Position = position,
                BuildingUnitId = BuildingUnitId,
                OsloId = OsloId,
                PositionWkbHex = PositionWkbHex,
                Function = Function,
                PositionMethod = PositionMethod,
                Status = Status,
                IsComplete = IsComplete,
                Version = Version,
                Addresses = new Collection<BuildingUnitAddressSyndicationItem>(Addresses.Select(x => x.CloneAndApplyEventInfo(position)).ToList()),
                Readdresses = new Collection<BuildingUnitReaddressSyndicationItem>(Readdresses.Select(x => x.CloneAndApplyEventInfo(position)).ToList()),
            };

            return newItem;
        }
    }

    public class BuildingUnitSyndicationItemConfiguration : IEntityTypeConfiguration<BuildingUnitSyndicationItem>
    {
        public const string TableName = "BuildingUnitSyndication";

        public void Configure(EntityTypeBuilder<BuildingUnitSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.Position, p.BuildingUnitId })
                .ForSqlServerIsClustered(false);

            b.Property(p => p.Position);
            b.Property(p => p.BuildingUnitId);
            b.Property(p => p.OsloId);
            b.Property(p => p.PositionWkbHex);

            b.Ignore(p => p.Version);
            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("Version");

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

            b.HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => new { x.Position, x.BuildingUnitId });

            b.HasMany(x => x.Readdresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => new { x.Position, x.BuildingUnitId });
        }
    }
}

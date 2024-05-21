namespace BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingUnitSyndicationItemV2
    {
        public long Position { get; set; }
        public int PersistentLocalId { get; set; }

        public byte[]? PointPosition { get; set; }
        public BuildingUnitPositionGeometryMethod PositionMethod { get; set; }

        public BuildingUnitFunction Function { get; set; }
        public BuildingUnitStatus Status { get; set; }

        public bool HasDeviation { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }
        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public Collection<BuildingUnitAddressSyndicationItemV2> Addresses { get; set; }

        public BuildingUnitSyndicationItemV2()
        {
            Addresses = new Collection<BuildingUnitAddressSyndicationItemV2>();
        }

        public BuildingUnitSyndicationItemV2 CloneAndApplyEventInfo(long position)
        {
            var newItem = new BuildingUnitSyndicationItemV2
            {
                Position = position,
                PersistentLocalId = PersistentLocalId,
                PointPosition = PointPosition,
                Function = Function,
                PositionMethod = PositionMethod,
                Status = Status,
                HasDeviation = HasDeviation,
                Version = Version,
                Addresses = new Collection<BuildingUnitAddressSyndicationItemV2>(Addresses.Select(x => x.CloneAndApplyEventInfo(position)).ToList()),
            };

            return newItem;
        }
    }

    public class BuildingUnitSyndicationItemV2Configuration : IEntityTypeConfiguration<BuildingUnitSyndicationItemV2>
    {
        private const string TableName = "BuildingUnitSyndicationV2WithCount";

        public void Configure(EntityTypeBuilder<BuildingUnitSyndicationItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.Position, p.PersistentLocalId })
                .IsClustered(false);

            b.Property(p => p.Position);

            b.HasIndex(p => new { p.Position, p.PersistentLocalId }).IsColumnStore($"CI_{TableName}_Position_BuildingUnitId");

            b.Property(p => p.PersistentLocalId);
            b.Property(p => p.PointPosition);

            b.Ignore(p => p.Version);
            b.Property(p => p.VersionTimestampAsDateTimeOffset)
                .HasColumnName("Version");

            b.Property(p => p.Status)
                .HasConversion(x => x.Status, y => BuildingUnitStatus.Parse(y));

            b.Property(p => p.HasDeviation)
                .HasDefaultValue(false);

            b.Property(p => p.PositionMethod)
                .HasConversion(x => x.GeometryMethod, y => BuildingUnitPositionGeometryMethod.Parse(y));

            b.Property(p => p.Function)
                .HasConversion(x => x.Function, y => BuildingUnitFunction.Parse(y));

            b.HasMany(x => x.Addresses)
                .WithOne()
                .IsRequired()
                .HasForeignKey(x => new { x.Position, x.BuildingUnitPersistentLocalId });
        }
    }
}

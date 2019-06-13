namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;
    using System;

    public class BuildingUnitReaddressSyndicationItem
    {
        public long Position { get; set; }
        public Guid BuildingUnitId { get; set; }
        public Guid OldAddressId { get; set; }
        public Guid NewAddressId { get; set; }
        public DateTime ReaddressBeginDateAsDateTimeOffset { get; set; }

        public LocalDate ReaddressBeginDate
        {
            get => LocalDate.FromDateTime(ReaddressBeginDateAsDateTimeOffset);
            set => ReaddressBeginDateAsDateTimeOffset = value.ToDateTimeUnspecified();
        }

        public BuildingUnitReaddressSyndicationItem CloneAndApplyEventInfo(long position)
        {
            var newItem = new BuildingUnitReaddressSyndicationItem
            {
                Position = position,
                BuildingUnitId = BuildingUnitId,
                OldAddressId = OldAddressId,
                NewAddressId = NewAddressId,
                ReaddressBeginDate = ReaddressBeginDate
            };

            return newItem;
        }
    }

    public class BuildingUnitReaddressSyndicationItemConfiguration : IEntityTypeConfiguration<BuildingUnitReaddressSyndicationItem>
    {
        public const string TableName = "BuildingUnitReaddressSyndication";

        public void Configure(EntityTypeBuilder<BuildingUnitReaddressSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.Position, p.BuildingUnitId, p.OldAddressId })
                .ForSqlServerIsClustered(false);

            b.Property(x => x.NewAddressId);
            b.Property(x => x.ReaddressBeginDateAsDateTimeOffset).HasColumnName("ReaddressDate");
            b.Ignore(x => x.ReaddressBeginDate);
        }
    }
}

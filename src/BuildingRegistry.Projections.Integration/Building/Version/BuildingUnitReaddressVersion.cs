namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using System;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NodaTime;

    public class BuildingUnitReaddressVersion
    {
        public long Position { get; set; }
        public int BuildingUnitPersistentLocalId { get; set; }
        public Guid OldAddressId { get; set; }
        public Guid NewAddressId { get; set; }
        public DateTime ReaddressBeginDateAsDateTimeOffset { get; set; }

        public LocalDate ReaddressBeginDate
        {
            get => LocalDate.FromDateTime(ReaddressBeginDateAsDateTimeOffset);
            set => ReaddressBeginDateAsDateTimeOffset = value.ToDateTimeUnspecified();
        }

        public BuildingUnitReaddressVersion CloneAndApplyEventInfo(long position)
        {
            var newItem = new BuildingUnitReaddressVersion
            {
                Position = position,
                BuildingUnitPersistentLocalId = BuildingUnitPersistentLocalId,
                OldAddressId = OldAddressId,
                NewAddressId = NewAddressId,
                ReaddressBeginDate = ReaddressBeginDate
            };

            return newItem;
        }
    }

    public class BuildingUnitReaddressSyndicationItemConfiguration : IEntityTypeConfiguration<BuildingUnitReaddressVersion>
    {
        public void Configure(EntityTypeBuilder<BuildingUnitReaddressVersion> builder)
        {
            const string tableName = "building_unit_readdress_versions";

            builder.ToTable(tableName, Schema.Integration)
                .HasKey(p => new { p.Position, p.BuildingUnitPersistentLocalId, p.OldAddressId })
                .IsClustered(false);

            builder.Property(x => x.Position).HasColumnName("position");
            builder.Property(x => x.BuildingUnitPersistentLocalId).HasColumnName("building_unit_persistent_id");
            builder.Property(x => x.NewAddressId).HasColumnName("new_address_id");
            builder.Property(x => x.OldAddressId).HasColumnName("new_address_id");
            builder.Property(x => x.ReaddressBeginDateAsDateTimeOffset)
                .HasColumnName("readdress_date")
                .HasColumnType("date");

            builder.Ignore(x => x.ReaddressBeginDate);
        }
    }
}

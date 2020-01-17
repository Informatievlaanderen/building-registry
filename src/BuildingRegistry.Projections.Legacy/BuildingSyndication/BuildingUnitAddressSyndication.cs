namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using System;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitAddressSyndicationItem
    {
        public long Position { get; set; }
        public Guid BuildingUnitId { get; set; }
        public Guid? AddressId { get; set; }
        public int Count { get; set; }

        public BuildingUnitAddressSyndicationItem CloneAndApplyEventInfo(long position)
        {
            var newItem = new BuildingUnitAddressSyndicationItem
            {
                Position = position,
                BuildingUnitId = BuildingUnitId,
                AddressId = AddressId,
                Count = Count
            };

            return newItem;
        }
    }

    public class BuildingUnitAddressSyndicationItemConfiguration : IEntityTypeConfiguration<BuildingUnitAddressSyndicationItem>
    {
        private const string TableName = "BuildingUnitAddressSyndication";

        public void Configure(EntityTypeBuilder<BuildingUnitAddressSyndicationItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.Position, p.BuildingUnitId, p.AddressId })
                .IsClustered(false);

            b.Property(x => x.Count);
        }
    }
}

namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetail
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using System;

    public class BuildingUnitDetailAddressItem
    {
        public Guid BuildingUnitId { get; set; }
        public Guid AddressId { get; set; }
        public int Count { get; set; } //Common units can have multiple of the same address coupled to it
    }

    public class BuildingUnitDetailAddressItemConfiguration : IEntityTypeConfiguration<BuildingUnitDetailAddressItem>
    {
        private const string TableName = "BuildingUnitAddresses";

        public void Configure(EntityTypeBuilder<BuildingUnitDetailAddressItem> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.BuildingUnitId, p.AddressId })
                .IsClustered(false);

            b.Property(x => x.Count);
        }
    }
}

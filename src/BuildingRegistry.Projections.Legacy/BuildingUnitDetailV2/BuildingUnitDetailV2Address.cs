namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitDetailAddressItemV2
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; } //Common units can have multiple of the same address coupled to it
    }

    public class BuildingUnitDetailAddressItemConfiguration : IEntityTypeConfiguration<BuildingUnitDetailAddressItemV2>
    {
        private const string TableName = "BuildingUnitAddressesV2";

        public void Configure(EntityTypeBuilder<BuildingUnitDetailAddressItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.BuildingUnitPersistentLocalId, p.AddressPersistentLocalId })
                .IsClustered();

            b.Property(x => x.Count);

            b.HasIndex(x => x.AddressPersistentLocalId).IsClustered(false);
            b.HasIndex(x => x.BuildingUnitPersistentLocalId).IsClustered(false);
        }
    }
}

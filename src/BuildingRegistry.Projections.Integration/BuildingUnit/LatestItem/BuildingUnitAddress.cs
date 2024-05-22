namespace BuildingRegistry.Projections.Integration.BuildingUnit.LatestItem
{
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddress
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }

        public BuildingUnitAddress()
        {
            Count = 1;
        }
    }

    public sealed class BuildingUnitAddressConfiguration : IEntityTypeConfiguration<BuildingUnitAddress>
    {
        public void Configure(EntityTypeBuilder<BuildingUnitAddress> builder)
        {
            const string tableName = "building_unit_addresses";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId });

            builder.Property(x => x.BuildingUnitPersistentLocalId).HasColumnName("building_unit_persistent_local_id");
            builder.Property(x => x.AddressPersistentLocalId).HasColumnName("address_persistent_local_id");
            builder.Property(e => e.Count).HasDefaultValue(1);

            builder.HasIndex(x => x.BuildingUnitPersistentLocalId);
            builder.HasIndex(x => x.AddressPersistentLocalId);
        }
    }
}

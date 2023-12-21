namespace BuildingRegistry.Projections.Integration
{
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddress
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }

        public BuildingUnitAddress()
        { }
    }

    public sealed class BuildingUnitAddressConfiguration : IEntityTypeConfiguration<BuildingUnitAddress>
    {
        public void Configure(EntityTypeBuilder<BuildingUnitAddress> builder)
        {
            const string tableName = "building_unit_addresses";

            builder
                .ToTable(tableName, Schema.Integration) // to schema per type
                .HasKey(x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId });
        }
    }
}

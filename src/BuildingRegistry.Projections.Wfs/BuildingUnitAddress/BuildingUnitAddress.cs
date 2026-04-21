namespace BuildingRegistry.Projections.Wfs.BuildingUnitAddress
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddress
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }

        private BuildingUnitAddress() { }

        public BuildingUnitAddress(int buildingUnitPersistentLocalId, int addressPersistentLocalId)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Count = 1;
        }
    }

    public sealed class BuildingUnitAddressConfiguration : IEntityTypeConfiguration<BuildingUnitAddress>
    {
        public const string TableName = "BuildingUnitAddresses";

        public void Configure(EntityTypeBuilder<BuildingUnitAddress> builder)
        {
            builder.ToTable(TableName, Schema.Wfs)
                .HasKey(x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId });

            builder.Property(x => x.Count)
                .HasDefaultValue(1)
                .ValueGeneratedNever();

            builder.HasIndex(x => x.AddressPersistentLocalId);
            builder.HasIndex(x => x.BuildingUnitPersistentLocalId);
        }
    }
}

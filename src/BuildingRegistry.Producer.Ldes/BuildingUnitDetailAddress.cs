namespace BuildingRegistry.Producer.Ldes
{
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitDetailAddress
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }

        private BuildingUnitDetailAddress()
        { }

        public BuildingUnitDetailAddress(int buildingUnitPersistentLocalId, int addressPersistentLocalId)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Count = 1;
        }
    }

    public class BuildingUnitDetailAddressConfiguration : IEntityTypeConfiguration<BuildingUnitDetailAddress>
    {
        private const string TableName = "BuildingUnitAddresses";

        public void Configure(EntityTypeBuilder<BuildingUnitDetailAddress> b)
        {
            b.ToTable(TableName, Schema.ProducerLdes)
                .HasKey(p => new { p.BuildingUnitPersistentLocalId, p.AddressPersistentLocalId })
                .IsClustered();

            b.Property(x => x.Count).HasDefaultValue(1);

            b.HasIndex(x => x.AddressPersistentLocalId);
            b.HasIndex(x => x.BuildingUnitPersistentLocalId);
        }
    }
}

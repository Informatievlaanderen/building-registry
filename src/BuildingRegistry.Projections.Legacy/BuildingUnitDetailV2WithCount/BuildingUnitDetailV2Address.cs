namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class BuildingUnitDetailAddressItemV2
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }

        private BuildingUnitDetailAddressItemV2()
        { }

        public BuildingUnitDetailAddressItemV2(int buildingUnitPersistentLocalId, int addressPersistentLocalId)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Count = 1;
        }
    }

    public class BuildingUnitDetailAddressItemConfiguration : IEntityTypeConfiguration<BuildingUnitDetailAddressItemV2>
    {
        private const string TableName = "BuildingUnitAddressesV2WithCount";

        public void Configure(EntityTypeBuilder<BuildingUnitDetailAddressItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => new { p.BuildingUnitPersistentLocalId, p.AddressPersistentLocalId })
                .IsClustered();

            b.Property(x => x.Count).HasDefaultValue(1);

            b.HasIndex(x => x.AddressPersistentLocalId).IsClustered(false);
            b.HasIndex(x => x.BuildingUnitPersistentLocalId).IsClustered(false);
        }
    }
}

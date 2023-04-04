namespace BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtract
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddressLinkExtractItem
    {
        public int BuildingPersistentLocalId { get; set; }
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public sealed class ParcelLinkExtractItemConfiguration : IEntityTypeConfiguration<BuildingUnitAddressLinkExtractItem>
    {
        private const string TableName = "BuildingUnitAddressLinks";

        public void Configure(EntityTypeBuilder<BuildingUnitAddressLinkExtractItem> builder)
        {
            builder.ToTable(TableName, Schema.Extract)
                .HasKey(p => new
                {
                    p.BuildingUnitPersistentLocalId, p.AddressPersistentLocalId
                })
                .IsClustered(false);

            builder.Property(p => p.DbaseRecord);
            builder.Property(p => p.BuildingPersistentLocalId);
            builder.Property(p => p.BuildingUnitPersistentLocalId);
            builder.Property(p => p.AddressPersistentLocalId);

            builder.HasIndex(p => p.BuildingPersistentLocalId);
            builder.HasIndex(p => p.AddressPersistentLocalId);
        }
    }
}


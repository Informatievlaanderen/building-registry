namespace BuildingRegistry.Projections.Extract.BuildingUnitAddressLinkExtractWithCount
{
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public sealed class BuildingUnitAddressLinkExtractItem
    {
        public int BuildingPersistentLocalId { get; set; }
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }
        public byte[] DbaseRecord { get; set; }
    }

    public sealed class ParcelLinkExtractItemConfiguration : IEntityTypeConfiguration<BuildingUnitAddressLinkExtractItem>
    {
        private const string TableName = "BuildingUnitAddressLinksWithCount";

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
            builder.Property(p => p.Count).HasDefaultValue(1);

            builder.HasIndex(p => p.BuildingPersistentLocalId);
            builder.HasIndex(p => p.AddressPersistentLocalId);
        }
    }
}


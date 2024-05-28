namespace BuildingRegistry.Consumer.Read.Parcel.Parcel
{
    using System;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelAddressItem
    {
        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }

        public ParcelAddressItem()
        {

        }

        public ParcelAddressItem(Guid parcelId, int addressPersistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
        }
    }

    public class ParcelAddressItemConfiguration : IEntityTypeConfiguration<ParcelAddressItem>
    {
        public const string TableName = "ParcelAddressItems";

        public void Configure(EntityTypeBuilder<ParcelAddressItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadParcel)
                .HasKey(x => new { x.ParcelId, x.AddressPersistentLocalId })
                .IsClustered(false);

            builder.HasIndex(x => x.ParcelId).IsClustered();
        }
    }
}

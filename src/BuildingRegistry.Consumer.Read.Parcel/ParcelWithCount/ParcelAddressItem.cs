namespace BuildingRegistry.Consumer.Read.Parcel.ParcelWithCount
{
    using System;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class ParcelAddressItem
    {
        public Guid ParcelId { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public int Count { get; set; }

        public ParcelAddressItem()
        { }

        public ParcelAddressItem(Guid parcelId, int addressPersistentLocalId)
        {
            ParcelId = parcelId;
            AddressPersistentLocalId = addressPersistentLocalId;
            Count = 1;
        }
    }

    public class ParcelAddressItemConfiguration : IEntityTypeConfiguration<ParcelAddressItem>
    {
        public const string TableName = "ParcelAddressItemsWithCount";

        public void Configure(EntityTypeBuilder<ParcelAddressItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadParcel)
                .HasKey(x => new { x.ParcelId, x.AddressPersistentLocalId })
                .IsClustered(false);

            builder.Property<int>(x => x.Count).HasDefaultValue(1);

            builder.HasIndex(x => x.ParcelId).IsClustered();
        }
    }
}

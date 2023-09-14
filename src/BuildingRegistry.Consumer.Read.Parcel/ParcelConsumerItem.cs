namespace BuildingRegistry.Consumer.Read.Parcel
{
    using System;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;

    public class ParcelConsumerItem
    {
        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public ParcelStatus Status { get; set; }
        public byte[] ExtendedWkbGeometry { get; set; }
        public Geometry Geometry { get; private set; }
        public bool IsRemoved { get; set; }

        //Needed for EF
        private ParcelConsumerItem()
        {
        }

        public ParcelConsumerItem(
            Guid parcelId,
            string caPaKey,
            ParcelStatus status,
            byte[] extendedWkbGeometry,
            Geometry geometry,
            bool isRemoved = false)
        {
            ParcelId = parcelId;
            CaPaKey = caPaKey;
            Status = status;
            ExtendedWkbGeometry = extendedWkbGeometry;
            SetGeometry(geometry);
            IsRemoved = isRemoved;
        }

        public void SetGeometry(Geometry geometry)
        {
            Geometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);
        }
    }

    public struct ParcelStatus
    {
        public static readonly ParcelStatus Realized = new ParcelStatus("Realized");
        public static readonly ParcelStatus Retired = new ParcelStatus("Retired");

        public string Status { get; }

        private ParcelStatus(string status) => Status = status;

        public static ParcelStatus Parse(string status)
        {
            if (status != Realized.Status &&
                status != Retired.Status)
            {
                throw new NotImplementedException($"Cannot parse {status} to ParcelStatus");
            }

            return new ParcelStatus(status);
        }

        public static implicit operator string(ParcelStatus status) => status.Status;
    }

    public class ParcelConsumerItemConfiguration : IEntityTypeConfiguration<ParcelConsumerItem>
    {
        public const string TableName = "ParcelItems";

        public void Configure(EntityTypeBuilder<ParcelConsumerItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadParcel)
                .HasKey(x => x.ParcelId)
                .IsClustered();

            builder.Property(x => x.CaPaKey);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.ExtendedWkbGeometry);
            builder.Property(p => p.Geometry).HasColumnType("sys.geometry");

            builder
                .Property(x => x.Status)
                .HasConversion(
                    addressStatus => addressStatus.Status,
                    status => ParcelStatus.Parse(status));

            builder.HasIndex(x => x.CaPaKey);
        }
    }
}

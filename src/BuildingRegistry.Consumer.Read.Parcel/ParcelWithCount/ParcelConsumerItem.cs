namespace BuildingRegistry.Consumer.Read.Parcel.ParcelWithCount
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;

    public class ParcelConsumerItem
    {
        /// <summary>
        /// The decimals a transformed geometry is rounded to. Geometries are persisted at centimetre
        /// precision and the transform is accurate to it. Only a transformed geometry is rounded; one that
        /// needs no transform is stored exactly as the event store holds it.
        /// </summary>
        private const int TransformedCoordinateDecimals = 2;

        public Guid ParcelId { get; set; }
        public string CaPaKey { get; set; }
        public ParcelStatus Status { get; set; }
        /// <summary>The bytes exactly as the parcel event store last published them, in whatever reference
        /// system that is. Nothing reads this column; that is what it would mean if anything did.</summary>
        public byte[] ExtendedWkbGeometry { get; set; }

        /// <summary>The geometry in Lambert 72 (EPSG 31370). Compared against while
        /// <see cref="Lambert2008ConversionCompletedToggle"/> is off. Dropped once it is on for good.</summary>
        public Geometry Geometry { get; private set; }

        /// <summary>The same geometry in Lambert 2008 (EPSG 3812). Null for rows not written since this
        /// column was added; the parcel register's conversion fills it for every parcel. See ADR 0006.</summary>
        public Geometry? GeometryLambert2008 { get; private set; }

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

        /// <summary>
        /// Fixes first, then transforms. <c>EnsureCoordinatesAreInCoordinateSystem</c> returns a geometry
        /// that is not <c>IsValid</c> untouched, so transforming an invalid parcel first would stamp an SRID
        /// onto unmoved coordinates. A fixed geometry is valid by construction. See ADR 0006.
        /// </summary>
        public void SetGeometry(Geometry geometry)
        {
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            if (!fixedGeometry.IsInsideFlandersUsingLambert72() && !fixedGeometry.IsInsideFlandersUsingLambert08())
            {
                throw new InvalidOperationException(
                    $"Parcel geometry (SRID {fixedGeometry.SRID}) lies outside Flanders in both Lambert 72 "
                    + "and Lambert 2008, so it cannot be transformed into either.");
            }

            Geometry = fixedGeometry.IsLambert72()
                ? fixedGeometry
                : fixedGeometry.EnsureLambert72().RoundCoordinates(TransformedCoordinateDecimals);

            GeometryLambert2008 = fixedGeometry.IsLambert08()
                ? fixedGeometry
                : fixedGeometry.EnsureLambert08(TransformedCoordinateDecimals);
        }

        /// <summary>
        /// Sets only the Lambert 2008 geometry, for the parcel register's CRS conversion. The parcel does not
        /// move there, it is re-expressed, so <see cref="Geometry"/> is already what it should be and
        /// transforming the payload back would replace it with a rounded round trip of itself. See ADR 0006.
        /// </summary>
        public void SetGeometryFromCrsConversion(Geometry geometry)
        {
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            GeometryLambert2008 = fixedGeometry.IsLambert08()
                ? fixedGeometry
                : fixedGeometry.EnsureLambert08(TransformedCoordinateDecimals);
        }

        /// <summary>The geometry in the reference system matching is done in.</summary>
        public Geometry? GeometryIn(int matchingSrid)
            => matchingSrid == SystemReferenceId.SridLambert2008 ? GeometryLambert2008 : Geometry;
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
        public const string TableName = "ParcelItemsWithCount";

        public void Configure(EntityTypeBuilder<ParcelConsumerItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerReadParcel)
                .HasKey(x => x.ParcelId)
                .IsClustered();

            builder.Property(x => x.CaPaKey);
            builder.Property(x => x.IsRemoved);
            builder.Property(x => x.ExtendedWkbGeometry);
            builder.Property(p => p.Geometry).HasColumnType("sys.geometry");
            builder.Property(p => p.GeometryLambert2008).HasColumnType("sys.geometry");

            builder
                .Property(x => x.Status)
                .HasConversion(
                    addressStatus => addressStatus.Status,
                    status => ParcelStatus.Parse(status));

            builder.HasIndex(x => x.CaPaKey);
        }
    }
}

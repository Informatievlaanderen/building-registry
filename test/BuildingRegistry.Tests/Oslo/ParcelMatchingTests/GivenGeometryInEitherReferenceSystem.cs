namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System;
    using System.Linq;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Consumer.Read.Parcel;
    using Consumer.Read.Parcel.ParcelWithCount;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;

    /// <summary>
    /// The parcel register and the building register are converted to Lambert 2008 on independent schedules,
    /// so matching has to bring both sides to one reference system whichever one each of them is in.
    ///
    /// Neither layer beneath would report a mismatch: SQL Server returns NULL rather than erroring, and NTS
    /// ignores SRID altogether and simply finds an empty intersection ~500 km away. A test that asserts the
    /// match still happens is therefore the only thing standing between a working join and a silent one.
    /// See ADR 0006.
    /// </summary>
    public class GivenGeometryInEitherReferenceSystem
    {
        private static readonly Lambert2008ConversionCompletedToggle Lambert72Matching = new(false);
        private static readonly Lambert2008ConversionCompletedToggle Lambert2008Matching = new(true);

        private readonly FakeConsumerParcelContext _context;

        public GivenGeometryInEitherReferenceSystem()
        {
            _context = new FakeConsumerParcelContextFactory().CreateDbContext([]);
        }

        [Fact]
        public void WhenBuildingIsLambert2008AndMatchingInLambert72_ThenItStillFindsTheParcel()
        {
            AddParcel(GeometryHelper.ValidPolygon);

            var matching = new ParcelMatching(_context, Lambert72Matching, new Lambert2008MatchingReadiness());

            var result = matching
                .GetUnderlyingParcels(WkbWriter.Instance.Write(GeometryHelper.ValidPolygonLambert2008))
                .ToList();

            result.Should().ContainSingle();
        }

        [Fact]
        public void WhenBuildingIsLambert72AndMatchingInLambert2008_ThenItStillFindsTheParcel()
        {
            AddParcel(GeometryHelper.ValidPolygon);

            var matching = new ParcelMatching(_context, Lambert2008Matching, new Lambert2008MatchingReadiness());

            var result = matching
                .GetUnderlyingParcels(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon))
                .ToList();

            result.Should().ContainSingle();
        }

        /// <summary>
        /// Both columns are populated on every write, so the toggle changes only which one is compared
        /// against, never whether the answer is right. That is what makes it safe to throw manually.
        /// </summary>
        [Fact]
        public void WhenAParcelIsStored_ThenItIsHeldInBothReferenceSystems()
        {
            var parcel = AddParcel(GeometryHelper.ValidPolygon);

            parcel.Geometry.SRID.Should().Be(SystemReferenceId.SridLambert72);
            parcel.GeometryLambert2008.Should().NotBeNull();
            parcel.GeometryLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);

            parcel.GeometryIn(SystemReferenceId.SridLambert72).Should().BeSameAs(parcel.Geometry);
            parcel.GeometryIn(SystemReferenceId.SridLambert2008).Should().BeSameAs(parcel.GeometryLambert2008);
        }

        [Fact]
        public void WhenAParcelArrivesInLambert2008_ThenItIsStoredVerbatimAndLambert72IsDerived()
        {
            var parcel = AddParcel(GeometryHelper.ValidPolygonLambert2008);

            parcel.GeometryLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            parcel.Geometry.SRID.Should().Be(SystemReferenceId.SridLambert72);

            // The same physical parcel, to within the centimetre a transformed geometry is rounded to.
            parcel.Geometry.Centroid.Distance(GeometryHelper.ValidPolygon.Centroid).Should().BeLessThan(0.05);
        }

        /// <summary>
        /// The one real hazard of a manually thrown toggle: matching in Lambert 2008 against a column that is
        /// not fully populated yet would silently skip those parcels.
        /// </summary>
        [Fact]
        public void WhenMatchingInLambert2008BeforeTheColumnIsPopulated_ThenItIsRefused()
        {
            var parcel = AddParcel(GeometryHelper.ValidPolygon);

            // A row as it would be before the parcel register's conversion reached it.
            _context.Entry(parcel).Property(nameof(ParcelConsumerItem.GeometryLambert2008)).CurrentValue = null;
            _context.SaveChanges();

            var matching = new ParcelMatching(_context, Lambert2008Matching, new Lambert2008MatchingReadiness());

            var act = () => matching.GetUnderlyingParcels(WkbWriter.Instance.Write(GeometryHelper.ValidPolygon)).ToList();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*no Lambert 2008 geometry*");
        }

        /// <summary>
        /// A geometry that lies outside Flanders in both systems is not transformed by either Ensure method,
        /// so it would just have an SRID stamped onto unmoved coordinates.
        /// </summary>
        [Fact]
        public void WhenAParcelLiesOutsideFlanders_ThenItIsRefused()
        {
            var outsideFlanders = GeometryHelper.ValidPolygonWithNoValidPoints;

            var act = () => AddParcel(outsideFlanders);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*outside Flanders*");
        }

        private ParcelConsumerItem AddParcel(Geometry geometry)
        {
            var parcel = new ParcelConsumerItem(
                Guid.NewGuid(),
                Guid.NewGuid().ToString(),
                ParcelStatus.Realized,
                WkbWriter.Instance.Write(geometry),
                geometry);

            _context.ParcelConsumerItemsWithCount.Add(parcel);
            _context.SaveChanges();

            return parcel;
        }
    }
}

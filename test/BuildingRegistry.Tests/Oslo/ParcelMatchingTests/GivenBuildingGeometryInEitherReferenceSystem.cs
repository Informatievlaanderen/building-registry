namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System;
    using System.Linq;
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetailV2;
    using Xunit;

    /// <summary>
    /// The building event store converts to Lambert 2008 gradually, so <c>BuildingDetailsV2</c> is written
    /// from both systems for the whole conversion window. Holding one column would make that window silently
    /// wrong in two ways at once: SQL Server returns NULL rather than erroring on an SRID mismatch, and the
    /// Lambert 72 spatial index's bounding box does not cover Lambert 2008 coordinates at all.
    ///
    /// A test that asserts the match still happens is the only thing standing between a working join and a
    /// silent one. See ADR 0006.
    /// </summary>
    public class GivenBuildingGeometryInEitherReferenceSystem
    {
        private static readonly Lambert2008ConversionCompletedToggle Lambert72Matching = new(false);
        private static readonly Lambert2008ConversionCompletedToggle Lambert2008Matching = new(true);

        private readonly FakeLegacyContext _legacyContext;

        public GivenBuildingGeometryInEitherReferenceSystem()
        {
            _legacyContext = new FakeLegacyContextFactory().CreateDbContext([]);
        }

        [Fact]
        public void WhenABuildingArrivesInLambert2008AndMatchingInLambert72_ThenItIsStillFound()
        {
            AddBuilding(GeometryHelper.ValidPolygonLambert2008);

            var matching = new BuildingMatching(_legacyContext, Lambert72Matching, new Lambert2008MatchingReadiness());

            var result = matching.GetUnderlyingBuildings(GeometryHelper.ValidPolygon).ToList();

            result.Should().ContainSingle();
        }

        [Fact]
        public void WhenABuildingArrivesInLambert72AndMatchingInLambert2008_ThenItIsStillFound()
        {
            AddBuilding(GeometryHelper.ValidPolygon);

            var matching = new BuildingMatching(_legacyContext, Lambert2008Matching, new Lambert2008MatchingReadiness());

            var result = matching.GetUnderlyingBuildings(GeometryHelper.ValidPolygon).ToList();

            result.Should().ContainSingle();
        }

        /// <summary>
        /// Both columns are written on every geometry write, so the toggle changes only which one is compared
        /// against, never whether the answer is right.
        /// </summary>
        [Fact]
        public void WhenABuildingIsStored_ThenItIsHeldInBothReferenceSystems()
        {
            var building = AddBuilding(GeometryHelper.ValidPolygon);

            building.SysGeometry!.SRID.Should().Be(SystemReferenceId.SridLambert72);
            building.SysGeometryLambert2008.Should().NotBeNull();
            building.SysGeometryLambert2008!.SRID.Should().Be(SystemReferenceId.SridLambert2008);

            building.SysGeometryIn(SystemReferenceId.SridLambert72).Should().BeSameAs(building.SysGeometry);
            building.SysGeometryIn(SystemReferenceId.SridLambert2008).Should().BeSameAs(building.SysGeometryLambert2008);
        }

        /// <summary>
        /// The one real hazard of a manually thrown toggle: matching in Lambert 2008 against a column that is
        /// not fully populated would silently skip those buildings.
        /// </summary>
        [Fact]
        public void WhenMatchingInLambert2008BeforeTheColumnIsPopulated_ThenItIsRefused()
        {
            var building = AddBuilding(GeometryHelper.ValidPolygon);

            // A row as it would be before the building event store's conversion reached it.
            _legacyContext.Entry(building)
                .Property(nameof(BuildingDetailItemV2.SysGeometryLambert2008)).CurrentValue = null;
            _legacyContext.SaveChanges();

            var matching = new BuildingMatching(_legacyContext, Lambert2008Matching, new Lambert2008MatchingReadiness());

            var act = () => matching.GetUnderlyingBuildings(GeometryHelper.ValidPolygon).ToList();

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*buildings*no Lambert 2008 geometry*");
        }

        /// <summary>
        /// A building whose geometry is not a polygon — the imported multipolygons — has always been stored
        /// with no <c>SysGeometry</c>. It has no Lambert 2008 geometry either, and must not be mistaken for a
        /// row the conversion has not reached: that would leave the guard tripped for good.
        /// </summary>
        [Fact]
        public void WhenABuildingHasNoPolygon_ThenBothColumnsAreNullAndTheGuardStillPasses()
        {
            var building = AddBuilding(GeometryHelper.ValidPolygon.Factory.CreateMultiPolygon([]));

            building.SysGeometry.Should().BeNull();
            building.SysGeometryLambert2008.Should().BeNull();
            building.SysGeometryIn(SystemReferenceId.SridLambert2008).Should().BeNull();

            _legacyContext.HasIncompleteLambert2008Geometry().Should().BeFalse();
        }

        /// <summary>
        /// A geometry outside Flanders in both systems is not transformed by either Ensure method — it is
        /// handed back as the same instance with an SRID stamped onto unmoved coordinates, which would leave
        /// both columns aliasing one wrongly labelled object.
        /// </summary>
        [Fact]
        public void WhenABuildingLiesOutsideFlanders_ThenItIsRefused()
        {
            var act = () => AddBuilding(GeometryHelper.ValidPolygonWithNoValidPoints);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*outside Flanders*");
        }

        private BuildingDetailItemV2 AddBuilding(Geometry geometry)
        {
            var building = new BuildingDetailItemV2(
                1,
                BuildingGeometryMethod.MeasuredByGrb,
                WkbWriter.Instance.Write(geometry),
                geometry,
                BuildingStatus.Realized,
                false,
                new Instant());

            _legacyContext.BuildingDetailsV2.Add(building);
            _legacyContext.SaveChanges();

            return building;
        }
    }
}

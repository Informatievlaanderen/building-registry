namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System.Linq;
    using BackOffice;
    using Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using Projections.Legacy;
    using Projections.Legacy.BuildingDetailV2;
    using Xunit;

    /// <summary>
    /// The other direction: a parcel geometry, arriving from the parcel register on its own conversion
    /// schedule, queried against building geometry that follows this repository's. It has to find the same
    /// buildings whichever system it turns up in, and nothing beneath would report a mismatch. See ADR 0006.
    ///
    /// This assumes <c>BuildingDetailV2.SysGeometry</c> is uniformly in the system matching is done in.
    /// That is a property of Projections.Legacy, which ADR 0005 left undecided.
    /// </summary>
    public class GivenParcelGeometryInEitherReferenceSystem
    {
        private static readonly Lambert2008ConversionCompletedToggle Lambert72Matching = new(false);
        private static readonly Lambert2008ConversionCompletedToggle Lambert2008Matching = new(true);

        private readonly FakeLegacyContext _legacyContext;

        public GivenParcelGeometryInEitherReferenceSystem()
        {
            _legacyContext = new FakeLegacyContextFactory().CreateDbContext([]);
        }

        [Fact]
        public void WhenParcelIsLambert2008AndMatchingInLambert72_ThenItStillFindsTheBuilding()
        {
            AddBuilding(GeometryHelper.ValidPolygon);

            var matching = new BuildingMatching(_legacyContext, Lambert72Matching);

            var result = matching.GetUnderlyingBuildings(GeometryHelper.ValidPolygonLambert2008).ToList();

            result.Should().ContainSingle();
        }

        [Fact]
        public void WhenParcelIsLambert72AndMatchingInLambert2008_ThenItStillFindsTheBuilding()
        {
            AddBuilding(GeometryHelper.ValidPolygonLambert2008);

            var matching = new BuildingMatching(_legacyContext, Lambert2008Matching);

            var result = matching.GetUnderlyingBuildings(GeometryHelper.ValidPolygon).ToList();

            result.Should().ContainSingle();
        }

        private void AddBuilding(Geometry geometry)
        {
            _legacyContext.BuildingDetailsV2.Add(new BuildingDetailItemV2(
                1,
                BuildingGeometryMethod.MeasuredByGrb,
                WkbWriter.Instance.Write(geometry),
                geometry,
                BuildingStatus.Realized,
                false,
                new Instant()));

            _legacyContext.SaveChanges();
        }
    }
}

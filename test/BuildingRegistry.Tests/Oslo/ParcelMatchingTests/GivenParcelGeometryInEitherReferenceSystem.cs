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
    /// The building side of that is held in two columns, one per system, so this no longer assumes anything
    /// about which one <c>BuildingDetailsV2</c> happens to be in — see
    /// <see cref="GivenBuildingGeometryInEitherReferenceSystem"/>.
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

            var matching = new BuildingMatching(_legacyContext, Lambert72Matching, new Lambert2008MatchingReadiness());

            var result = matching.GetUnderlyingBuildings(GeometryHelper.ValidPolygonLambert2008).ToList();

            result.Should().ContainSingle();
        }

        [Fact]
        public void WhenParcelIsLambert72AndMatchingInLambert2008_ThenItStillFindsTheBuilding()
        {
            AddBuilding(GeometryHelper.ValidPolygonLambert2008);

            var matching = new BuildingMatching(_legacyContext, Lambert2008Matching, new Lambert2008MatchingReadiness());

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

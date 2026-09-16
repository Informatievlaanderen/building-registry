namespace BuildingRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;
    // GrAr's reader rather than BuildingRegistry's, deliberately: this one throws on EWKB without an SRID
    // instead of falling back to Lambert 72, so reading the bytes back also proves the SRID is in them.
    using StrictEwkbReaderFactory = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.WKBReaderFactory;

    /// <summary>
    /// What the lambda persists. Geometries reaching it have already been normalized to the event store's
    /// reference system by the API (ADR 0003), so this reads the srsName rather than assuming one, and
    /// records the SRID in the EWKB it writes.
    ///
    /// A building geometry and a building unit position are not written the same way: the outline keeps
    /// every decimal it came with, the position is rounded to centimetres. See ADR 0007.
    /// </summary>
    public class GmlHelpersTests
    {
        /// <summary>
        /// <see cref="GeometryHelper.GmlPointGeometryLambert2008"/> with millimetres and finer on it. The
        /// normalizer passes a geometry already in the event store's reference system through verbatim, so
        /// this is what an over-precise request looks like by the time the lambda sees it.
        /// </summary>
        private const string OverPreciseGmlPointLambert2008 =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>641296.9712345 685187.3587654</gml:pos></gml:Point>";

        private const string OverPreciseGmlPointLambert72 =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>141299.0012345 185188.0012345</gml:pos></gml:Point>";

        private const string GmlPointWithoutSrsName =
            "<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>141299.00 185188.00</gml:pos></gml:Point>";

        private const string GmlPointInWgs84 =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>4.35 51.21</gml:pos></gml:Point>";

        [Fact]
        public void GivenLambert72Position_ThenTheEwkbCarriesLambert72()
        {
            var position = ReadBackPosition(GeometryHelper.GmlPointGeometry);

            position.SRID.Should().Be(SystemReferenceId.SridLambert72);
            position.X.Should().Be(141299.00);
            position.Y.Should().Be(185188.00);
        }

        [Fact]
        public void GivenLambert2008Position_ThenTheEwkbCarriesLambert2008()
        {
            var position = ReadBackPosition(GeometryHelper.GmlPointGeometryLambert2008);

            position.SRID.Should().Be(SystemReferenceId.SridLambert2008);
            position.X.Should().Be(641296.97);
            position.Y.Should().Be(685187.36);
        }

        /// <summary>
        /// The SRID has to be in the bytes: without it every reader falls back to Lambert 72, which is a
        /// silent ~500 km error once the event store holds Lambert 2008.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometry)]
        [InlineData(GeometryHelper.GmlPointGeometryLambert2008)]
        public void ThenTheSridIsPersisted(string gml)
        {
            gml.ToExtendedWkbPosition()
                .ToByteArray()
                .TryReadSrid(out _)
                .Should().BeTrue();
        }

        /// <summary>
        /// The event store holds building unit positions at centimetre precision and nothing finer gets in.
        /// A caller cannot reproduce a millimetre from what the API serves — every reader rounds a position
        /// to two decimals — so persisting one means the position can never be posted back as it was stored.
        /// </summary>
        [Theory]
        [InlineData(OverPreciseGmlPointLambert2008, 641296.97, 685187.36)]
        [InlineData(OverPreciseGmlPointLambert72, 141299.00, 185188.00)]
        public void GivenAPositionBeyondCentimetrePrecision_ThenItIsRoundedToCentimetres(
            string gml,
            double expectedX,
            double expectedY)
        {
            var position = ReadBackPosition(gml);

            position.X.Should().Be(expectedX);
            position.Y.Should().Be(expectedY);
        }

        /// <summary>
        /// And the rounded position is byte-identical to the one the same point sent at centimetre
        /// precision produces, so the two requests are the same edit rather than two different ones.
        /// </summary>
        [Theory]
        [InlineData(OverPreciseGmlPointLambert2008, GeometryHelper.GmlPointGeometryLambert2008)]
        [InlineData(OverPreciseGmlPointLambert72, GeometryHelper.GmlPointGeometry)]
        public void GivenAPositionBeyondCentimetrePrecision_ThenTheEwkbIsTheSameAsAtCentimetrePrecision(
            string overPrecise,
            string atCentimetrePrecision)
        {
            overPrecise.ToExtendedWkbPosition()
                .Should().Be(atCentimetrePrecision.ToExtendedWkbPosition());
        }

        /// <summary>
        /// The other half of the asymmetry: a building geometry keeps every decimal it came with. Rounding
        /// an outline's vertices would move the boundary rather than tidy it. See ADR 0007.
        /// </summary>
        [Fact]
        public void GivenABuildingGeometry_ThenItsVerticesAreNotRounded()
        {
            var geometry = ReadBack(GeometryHelper.GmlPolygonGeometry, gml => gml.ToExtendedWkbGeometry());

            geometry.Coordinates[0].X.Should().Be(141298.83027724177);
            geometry.Coordinates[0].Y.Should().Be(185196.03552261367);
        }

        /// <summary>Rounding is not allowed to reach the caller's geometry: it rounds in place.</summary>
        [Fact]
        public void ThenTheCallersGeometryIsNotRounded()
        {
            var geometry = OverPreciseGmlPointLambert2008.ReadGeometry();

            BuildingRegistry.Building.ExtendedWkbGeometry.CreatePosition(geometry);

            geometry.Coordinates[0].X.Should().Be(641296.9712345);
        }

        [Theory]
        [InlineData(GmlPointWithoutSrsName)]
        [InlineData(GmlPointInWgs84)]
        public void GivenAnUnsupportedOrMissingSrsName_ThenThrows(string gml)
        {
            var act = () => gml.ToExtendedWkbPosition();

            act.Should().Throw<InvalidOperationException>();
        }

        private static Point ReadBackPosition(string gml)
            => (Point)ReadBack(gml, x => x.ToExtendedWkbPosition());

        private static Geometry ReadBack(
            string gml,
            Func<string, BuildingRegistry.Building.ExtendedWkbGeometry> toEwkb)
        {
            var ewkb = toEwkb(gml).ToByteArray();
            return StrictEwkbReaderFactory.CreateForEwkb(ewkb).Read(ewkb);
        }
    }
}

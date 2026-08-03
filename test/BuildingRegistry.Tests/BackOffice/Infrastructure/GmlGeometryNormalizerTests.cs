namespace BuildingRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Infrastructure;
    using FluentAssertions;
    using Xunit;

    public class GmlGeometryNormalizerTests
    {
        private const string HttpSrsNameGmlPointGeometry = GeometryHelper.NormalizedGmlPointGeometry;
        private const string HttpSrsNameGmlPointGeometryLambert2008 = GeometryHelper.NormalizedGmlPointGeometryLambert2008;
        private const string HttpSrsNameGmlPolygonGeometryLambert2008 = GeometryHelper.NormalizedGmlPolygonGeometryLambert2008;

        private static GmlGeometryNormalizer WithLambert2008EventStore(bool enabled)
            => new GmlGeometryNormalizer(new UseLambert2008EventStoreToggle(enabled));

        /// <summary>
        /// A geometry already in the event store's reference system is passed through verbatim,
        /// srsName scheme and coordinate precision included.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometry)]
        [InlineData(HttpSrsNameGmlPointGeometry)]
        [InlineData(GeometryHelper.GmlPolygonGeometry)]
        public void GivenLambert72EventStore_WhenLambert72Geometry_ThenGeometryIsUnchanged(string geometry)
        {
            WithLambert2008EventStore(false)
                .ToEventStoreSrs(geometry)
                .Should().Be(geometry);
        }

        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometryLambert2008)]
        [InlineData(HttpSrsNameGmlPointGeometryLambert2008)]
        [InlineData(GeometryHelper.GmlPolygonGeometryLambert2008)]
        public void GivenLambert2008EventStore_WhenLambert2008Geometry_ThenGeometryIsUnchanged(string geometry)
        {
            WithLambert2008EventStore(true)
                .ToEventStoreSrs(geometry)
                .Should().Be(geometry);
        }

        /// <summary>
        /// A converted geometry is re-serialized to a single canonical form with an http srsName,
        /// regardless of the scheme it was sent with.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometryLambert2008, GeometryHelper.NormalizedGmlPointGeometry)]
        [InlineData(HttpSrsNameGmlPointGeometryLambert2008, GeometryHelper.NormalizedGmlPointGeometry)]
        [InlineData(GeometryHelper.GmlPolygonGeometryLambert2008, GeometryHelper.NormalizedGmlPolygonGeometry)]
        [InlineData(HttpSrsNameGmlPolygonGeometryLambert2008, GeometryHelper.NormalizedGmlPolygonGeometry)]
        public void GivenLambert72EventStore_WhenLambert2008Geometry_ThenGeometryIsConvertedToLambert72(
            string geometry,
            string expected)
        {
            WithLambert2008EventStore(false)
                .ToEventStoreSrs(geometry)
                .ShouldBeEquivalentGml(expected);
        }

        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometry, GeometryHelper.NormalizedGmlPointGeometryLambert2008)]
        [InlineData(HttpSrsNameGmlPointGeometry, GeometryHelper.NormalizedGmlPointGeometryLambert2008)]
        [InlineData(GeometryHelper.GmlPolygonGeometry, GeometryHelper.NormalizedGmlPolygonGeometryLambert2008)]
        public void GivenLambert2008EventStore_WhenLambert72Geometry_ThenGeometryIsConvertedToLambert2008(
            string geometry,
            string expected)
        {
            WithLambert2008EventStore(true)
                .ToEventStoreSrs(geometry)
                .ShouldBeEquivalentGml(expected);
        }

        [Theory]
        [InlineData("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>4.35 50.85</gml:pos></gml:Point>")]
        [InlineData("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>103671.37 192046.71</gml:pos></gml:Point>")]
        [InlineData("<gml:Point missingSrSNameAttribute=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>103671.37 192046.71</gml:pos></gml:Point>")]
        public void WhenSrsNameIsUnsupported_ThenThrows(string gml)
        {
            var act = () => WithLambert2008EventStore(false).ToEventStoreSrs(gml);

            act.Should().Throw<InvalidOperationException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void WhenGeometryIsAbsent_ThenItIsPassedThrough(string? gml)
        {
            WithLambert2008EventStore(false)
                .ToEventStoreSrsWhenPresent(gml)
                .Should().Be(gml);
        }
    }
}

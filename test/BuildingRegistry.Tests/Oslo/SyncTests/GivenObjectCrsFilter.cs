namespace BuildingRegistry.Tests.Oslo.SyncTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using Api.BackOffice.Abstractions.Building;
    using Api.Oslo.Building.V2.Query;
    using Api.Oslo.Building.V2.Sync;
    using Api.Oslo.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using BuildingRegistry.Projections.Legacy.BuildingSyndicationWithCount;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed.Atom;
    using NodaTime;
    using Xunit;
    using BuildingGeometryMethod = BuildingRegistry.Legacy.BuildingGeometryMethod;
    using BuildingStatus = BuildingRegistry.Legacy.BuildingStatus;

    /// <summary>
    /// The objectCrs filter selects the reference system of the embedded object only. The embedded event is
    /// always emitted exactly as the event store held it at that position. See ADR 0004.
    /// </summary>
    public class GivenObjectCrsFilter
    {
        // First vertex of GeometryHelper.ValidPolygon and of GeometryHelper.GmlPolygonGeometryLambert2008,
        // which is the same polygon expressed in Lambert 2008.
        private const double FirstXLambert72 = 141298.83027724177;
        private const double FirstXLambert2008 = 641296.80075767275;

        // GeometryHelper.GmlPointGeometry and its Lambert 2008 counterpart.
        private const double PointXLambert72 = 141299.00;
        private const double PointXLambert2008 = 641296.97;

        /// <summary>
        /// The object's GML carries no srsName — neither GmlPolygon nor GmlPoint has such a member — so the
        /// coordinates are the only evidence of which reference system the object came back in. They are read
        /// numerically because the posList is rendered at 11 decimals, where 641296.80075767275 shows up as
        /// 641296.80075767252.
        /// </summary>
        private static double FirstXOfObjectGeometry(string feed)
        {
            var posList = Regex.Match(feed, "<posList>([^<]+)</posList>");
            posList.Success.Should().BeTrue("the feed should contain an object geometry");

            return double.Parse(posList.Groups[1].Value.Split(' ')[0], CultureInfo.InvariantCulture);
        }

        private static double XOfObjectUnitPosition(string feed)
        {
            var pos = Regex.Match(feed, "<pos>([^<]+)</pos>");
            pos.Success.Should().BeTrue("the feed should contain a building unit position");

            return double.Parse(pos.Groups[1].Value.Split(' ')[0], CultureInfo.InvariantCulture);
        }

        private static byte[] ToExtendedWkb(string gml)
            => WkbWriter.Instance.Write(gml.ReadGeometry());

        private static byte[] Lambert72Polygon() => ToExtendedWkb(GeometryHelper.GmlPolygonGeometry);
        private static byte[] Lambert2008Polygon() => ToExtendedWkb(GeometryHelper.GmlPolygonGeometryLambert2008);
        private static byte[] Lambert72Point() => ToExtendedWkb(GeometryHelper.GmlPointGeometry);
        private static byte[] Lambert2008Point() => ToExtendedWkb(GeometryHelper.GmlPointGeometryLambert2008);

        private static async Task<string> WriteFeed(byte[] polygon, byte[] point, string? objectCrs)
        {
            var timestamp = Instant.FromUtc(2026, 1, 1, 0, 0);

            var building = new BuildingSyndicationQueryResult(
                Guid.NewGuid().ToString("D"),
                1,
                123,
                BuildingStatus.Realized,
                BuildingGeometryMethod.MeasuredByGrb,
                polygon,
                "BuildingWasMeasured",
                timestamp,
                timestamp,
                true,
                null,
                "reason",
                new List<BuildingUnitSyndicationItemV2>
                {
                    new BuildingUnitSyndicationItemV2
                    {
                        Position = 1,
                        PersistentLocalId = 456,
                        PointPosition = point,
                        PositionMethod = BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                        Function = BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                        Status = BuildingRegistry.Building.BuildingUnitStatus.Realized,
                        HasDeviation = false,
                        Version = timestamp
                    }
                },
                // The event payload carries the store's own hex, and must come out untouched whatever objectCrs says.
                $"<BuildingWasMeasured><ExtendedWkbGeometry>{Convert.ToHexString(polygon)}</ExtendedWkbGeometry></BuildingWasMeasured>");

            var sw = new StringWriterWithEncoding(Encoding.UTF8);
            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);

                await writer.WriteBuilding(
                    new OptionsWrapper<ResponseOptionsV2>(new ResponseOptionsV2
                    {
                        GebouwNaamruimte = "https://data.vlaanderen.be/id/gebouw",
                        GebouweenheidNaamruimte = "https://data.vlaanderen.be/id/gebouweenheid"
                    }),
                    formatter,
                    "category1",
                    "category2",
                    building,
                    ObjectCrs.ToSrid(objectCrs));

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        [Theory]
        [InlineData("3812", SystemReferenceId.SridLambert2008)]
        [InlineData(" 3812 ", SystemReferenceId.SridLambert2008)]
        [InlineData("31370", SystemReferenceId.SridLambert72)]
        [InlineData("EPSG:3812", SystemReferenceId.SridLambert72)]
        [InlineData("nonsense", SystemReferenceId.SridLambert72)]
        [InlineData("", SystemReferenceId.SridLambert72)]
        [InlineData(null, SystemReferenceId.SridLambert72)]
        public void ThenOnlyTheExactValue3812SelectsLambert2008(string? objectCrs, int expectedSrid)
            => ObjectCrs.ToSrid(objectCrs).Should().Be(expectedSrid);

        /// <summary>
        /// The default path is a pass-through, not a round trip: the posList is asserted at its full 11
        /// decimals, which is what would be lost if an already Lambert 72 geometry were rounded on the way out.
        /// </summary>
        [Fact]
        public async Task WhenNotRequested_ThenLambert72SourceIsUnchanged()
        {
            var feed = await WriteFeed(Lambert72Polygon(), Lambert72Point(), objectCrs: null);

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
            XOfObjectUnitPosition(feed).Should().BeApproximately(PointXLambert72, 0.01);

            feed.Should().Contain(FirstXLambert72.ToString("F11", CultureInfo.InvariantCulture));
        }

        [Fact]
        public async Task WhenRequesting3812_ThenLambert72SourceIsConverted()
        {
            var feed = await WriteFeed(Lambert72Polygon(), Lambert72Point(), objectCrs: "3812");

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert2008, 0.01);
            XOfObjectUnitPosition(feed).Should().BeApproximately(PointXLambert2008, 0.01);
        }

        [Fact]
        public async Task WhenRequesting3812_ThenLambert2008SourceStaysAsIs()
        {
            var feed = await WriteFeed(Lambert2008Polygon(), Lambert2008Point(), objectCrs: "3812");

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert2008, 0.01);
            XOfObjectUnitPosition(feed).Should().BeApproximately(PointXLambert2008, 0.01);
        }

        /// <summary>
        /// The default direction, and the one that only starts mattering once the event store is converted:
        /// a caller that does not ask keeps getting Lambert 72, so the feed's existing contract holds.
        /// </summary>
        [Fact]
        public async Task WhenNotRequested_ThenLambert2008SourceIsConvertedBackToLambert72()
        {
            var feed = await WriteFeed(Lambert2008Polygon(), Lambert2008Point(), objectCrs: null);

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
            XOfObjectUnitPosition(feed).Should().BeApproximately(PointXLambert72, 0.01);
        }

        [Fact]
        public async Task WhenUnrecognisedValue_ThenLambert2008SourceIsConvertedBackToLambert72()
        {
            var feed = await WriteFeed(Lambert2008Polygon(), Lambert2008Point(), objectCrs: "nonsense");

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
            XOfObjectUnitPosition(feed).Should().BeApproximately(PointXLambert72, 0.01);
        }

        /// <summary>
        /// The embedded event is the event store's own payload and is never reprojected, even when the object
        /// beside it is.
        /// </summary>
        [Fact]
        public async Task WhenRequesting3812_ThenTheEmbeddedEventIsStillTheStoredGeometry()
        {
            var stored = Lambert72Polygon();

            var feed = await WriteFeed(stored, Lambert72Point(), objectCrs: "3812");

            feed.Should().Contain(Convert.ToHexString(stored));
            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert2008, 0.01);
        }

        /// <summary>
        /// Geometries persisted before the event store recorded an SRID carry no SRID at all, and are read as
        /// Lambert 72 — the single place that decision lives is <c>BuildingRegistry.WKBReaderFactory.CreateForEwkb</c>.
        /// </summary>
        [Fact]
        public async Task WhenPersistedWithoutSrid_ThenItIsReadAsLambert72()
        {
            var withoutSrid = new NetTopologySuite.IO.WKBWriter().Write(GeometryHelper.GmlPolygonGeometry.ReadGeometry());

            var feed = await WriteFeed(withoutSrid, Lambert72Point(), objectCrs: null);

            FirstXOfObjectGeometry(feed).Should().BeApproximately(FirstXLambert72, 0.01);
        }
    }
}

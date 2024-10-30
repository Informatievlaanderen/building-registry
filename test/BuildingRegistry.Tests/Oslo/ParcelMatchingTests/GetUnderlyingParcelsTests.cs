namespace BuildingRegistry.Tests.Oslo.ParcelMatchingTests
{
    using System;
    using System.Linq;
    using Api.BackOffice.Abstractions.Building;
    using BackOffice;
    using BuildingRegistry.Legacy;
    using Consumer.Read.Parcel;
    using Consumer.Read.Parcel.ParcelWithCount;
    using FluentAssertions;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using Xunit;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;
    using GeometryFactory = BuildingRegistry.GeometryFactory;

    public class GetUnderlyingParcelsTests
    {
        private readonly FakeConsumerParcelContext _consumerParcelContext;

        public GetUnderlyingParcelsTests()
        {
            _consumerParcelContext = new FakeConsumerParcelContextFactory()
                .CreateDbContext([]);
        }

        [Fact]
        public void WithParcelOverlapping100Percent_ThenReturnsTheUnderlyingParcel()
        {
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingGeometry100PercentOverlap = parcelGeometry;

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry100PercentOverlap.AsBinary());

            result.Should().ContainSingle();
        }

        [Fact]
        public void WithRetiredParcelOverlapping100Percent_ThenReturnsNothing()
        {
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingGeometry100PercentOverlap = parcelGeometry;

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Retired,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry100PercentOverlap.AsBinary());

            result.Should().BeEmpty();
        }

        [Fact]
        public void WithParcelLessThan80PercentOverlap_ThenReturnsNothing()
        {
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var buildingGeometry = CreateGeometry("140 100 140 200 240 200 240 100 140 100");

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry.AsBinary());

            result.Should().BeEmpty();
        }

        [Fact]
        public void With2ParcelsAbove40PercentOverlap_ThenReturnsThe2Parcels()
        {
            var buildingGeometry50PercentOverlap = CreateGeometry("50 100 50 200 140 200 140 100 50 100");
            var parcelGeometry = CreateGeometry("100 100 100 200 200 200 200 100 100 100");

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelGeometry.AsBinary(),
                    parcelGeometry));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry50PercentOverlap.AsBinary());

            result.Count().Should().Be(2);
        }

        [Fact]
        public void With2Parcels_1Above40Percent_1Under40Percent_ThenReturns1Parcel()
        {
            var parcelLeft = CreateGeometry("100 100 100 200 200 200 200 100 100 100");
            var parcelLeftCaPaKey = Guid.NewGuid().ToString();
            var parcelRight = CreateGeometry("200 100 200 200 300 200 300 100 200 100");
            var building = CreateGeometry("139 100 139 200 239 200 239 100 139 100");

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    parcelLeftCaPaKey,
                    ParcelStatus.Realized,
                    parcelLeft.AsBinary(),
                    parcelLeft));

            _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                new ParcelConsumerItem(
                    Guid.NewGuid(),
                    Guid.NewGuid().ToString(),
                    ParcelStatus.Realized,
                    parcelRight.AsBinary(),
                    parcelRight));

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext);

            var result = parcelMatching.GetUnderlyingParcels(building.AsBinary()).ToList();

            result.Count.Should().Be(1);
            result.First().Should().Be(parcelLeftCaPaKey);
        }

        [Fact]
        public void WithSideLocationConflict_ThenReturnsTheUnderlyingParcel()
        {
            var parcelGeometriesWkt = new[]
            {
                "POLYGON ((72034.0887893587 169823.674295988, 72033.3051733598 169823.17842399, 72024.276565358 169837.717495997, 72022.6098133549 169840.401464, 72021.27061335 169842.558200002, 72024.9522133544 169845.027064003, 72029.565205358 169848.118008006, 72032.0142933577 169849.783608008, 72034.1940053627 169851.265976008, 72035.0398293659 169850.256504007, 72036.2742613629 169848.783288006, 72038.3694293648 169846.175736003, 72040.6023893654 169843.396792002, 72042.6674773693 169840.826680001, 72044.8400213718 169838.122871999, 72046.4402773678 169836.131319996, 72048.9422293752 169833.073079996, 72047.4212693721 169832.110647995, 72046.8450773731 169831.746039994, 72042.6574293673 169829.096183993, 72041.4310613647 169828.320183992, 72040.1595733687 169827.515703991, 72035.5725653619 169824.613111988, 72035.2214613631 169824.390967987, 72034.6434773654 169824.025207989, 72034.4682453647 169823.914359987, 72034.0887893587 169823.674295988))",
                "POLYGON ((72066.7159893811 169827.166519992, 72061.1319893822 169823.665015988, 72056.8332373798 169830.698743992, 72053.5861973763 169836.011639997, 72059.0299093798 169839.456248, 72059.5551573783 169839.788663998, 72062.922965385 169834.420599997, 72067.2597973868 169827.507831991, 72066.7159893811 169827.166519992))",
                "POLYGON ((72072.54299739 169830.839031994, 72072.0604373887 169830.535415992, 72069.5579733849 169834.526007995, 72069.272021383 169834.982071996, 72067.4630613849 169837.866743997, 72065.9008213803 169843.803960003, 72068.1579733863 169839.002999999, 72067.6599893868 169838.671992, 72067.7493333817 169838.527223997, 72070.0389973894 169834.828023996, 72072.54299739 169830.839031994))",
                "POLYGON ((72065.9008213803 169843.803960003, 72059.5551573783 169839.788663998, 72059.0299093798 169839.456248, 72053.5861973763 169836.011639997, 72053.4361813739 169835.916727997, 72053.031957373 169835.660919998, 72050.6801493764 169834.172791995, 72048.9422293752 169833.073079996, 72046.4402773678 169836.131319996, 72051.5150933713 169840.226296, 72052.4039893746 169841.212919999, 72060.9487573802 169850.697592009, 72063.8411733806 169848.184184004, 72063.9414613843 169848.223416004, 72065.9008213803 169843.803960003))",
                "POLYGON ((72040.6429653689 169843.442936003, 72040.6023893654 169843.396792002, 72038.3694293648 169846.175736003, 72038.3921493664 169846.201080006, 72041.2276693657 169849.358904008, 72044.7431253716 169853.273912009, 72050.4692693725 169859.651000012, 72053.0146133751 169857.470200013, 72044.5769813657 169847.903416004, 72043.8007253706 169847.023288004, 72040.6429653689 169843.442936003))",
                "POLYGON ((72070.5003093854 169829.543863993, 72067.2597973868 169827.507831991, 72062.922965385 169834.420599997, 72059.5551573783 169839.788663998, 72065.9008213803 169843.803960003, 72067.4630613849 169837.866743997, 72069.272021383 169834.982071996, 72069.5579733849 169834.526007995, 72072.0604373887 169830.535415992, 72071.8896213844 169830.427959993, 72070.5003093854 169829.543863993))",
                "POLYGON ((72051.5150933713 169840.226296, 72046.4402773678 169836.131319996, 72044.8400213718 169838.122871999, 72044.8513493687 169838.135479998, 72048.8644053712 169842.596984003, 72049.4645973742 169843.264312003, 72049.6990933716 169843.525048003, 72058.231829375 169853.01132001, 72060.9487573802 169850.697592009, 72052.4039893746 169841.212919999, 72051.5150933713 169840.226296))",
                "POLYGON ((72044.8513493687 169838.135479998, 72044.8400213718 169838.122871999, 72042.6674773693 169840.826680001, 72042.6939093694 169840.85644, 72045.8939733729 169844.441976003, 72046.2299733683 169844.818360005, 72047.0765013695 169845.766840003, 72055.5700053796 169855.283448011, 72058.231829375 169853.01132001, 72049.6990933716 169843.525048003, 72049.4645973742 169843.264312003, 72048.8644053712 169842.596984003, 72044.8513493687 169838.135479998))",
                "POLYGON ((72042.6939093694 169840.85644, 72042.6674773693 169840.826680001, 72040.6023893654 169843.396792002, 72040.6429653689 169843.442936003, 72043.8007253706 169847.023288004, 72044.5769813657 169847.903416004, 72053.0146133751 169857.470200013, 72055.5700053796 169855.283448011, 72047.0765013695 169845.766840003, 72046.2299733683 169844.818360005, 72045.8939733729 169844.441976003, 72042.6939093694 169840.85644))",
                "POLYGON ((72060.703253381 169823.395703986, 72055.1049173772 169819.879031986, 72050.6731733754 169826.93387999, 72047.4212693721 169832.110647995, 72048.9422293752 169833.073079996, 72050.6801493764 169834.172791995, 72053.031957373 169835.660919998, 72053.4361813739 169835.916727997, 72053.5861973763 169836.011639997, 72056.8332373798 169830.698743992, 72061.1319893822 169823.665015988, 72060.703253381 169823.395703986))",
            };
            var buildingGeometry = CreateGeometry("72065.90082138032 169843.80396000296 72063.94146138430 169848.22341600433 72063.84117338061 169848.18418400362 72060.94875738025 169850.69759200886 72052.40398937464 169841.21291999891 72051.51509337127 169840.22629600018 72046.44027736783 169836.13131999597 72048.94222937524 169833.07307999581 72050.68014937639 169834.17279199511 72053.03195737302 169835.66091999784 72053.43618137389 169835.91672799736 72053.58619737625 169836.01163999736 72059.02990937978 169839.45624800026 72059.55515737832 169839.78866399825 72065.90082138032 169843.80396000296");

            foreach (var parcelGeometry in parcelGeometriesWkt)
            {
                var geometry = new WKTReader(GeometryFactory.CreateNtsGeometryServices())
                    .Read(parcelGeometry);

                _consumerParcelContext.ParcelConsumerItemsWithCount.Add(
                    new ParcelConsumerItem(
                        Guid.NewGuid(),
                        Guid.NewGuid().ToString(),
                        ParcelStatus.Realized,
                        geometry.AsBinary(),
                        geometry));
            }

            _consumerParcelContext.SaveChanges();

            var parcelMatching = new ParcelMatching(_consumerParcelContext);

            var result = parcelMatching.GetUnderlyingParcels(buildingGeometry.AsBinary());

            result.Should().ContainSingle();
        }

        private static Geometry CreateGeometry(string coordinates)
            => GmlHelpers.CreateGmlReader().Read(
                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                "<gml:exterior>" +
                "<gml:LinearRing>" +
                "<gml:posList>" +
                coordinates +
                "</gml:posList>" +
                "</gml:LinearRing>" +
                "</gml:exterior>" +
                "</gml:Polygon>");
    }
}

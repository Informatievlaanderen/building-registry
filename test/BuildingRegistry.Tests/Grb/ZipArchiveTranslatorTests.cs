namespace BuildingRegistry.Tests.Grb
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BuildingRegistry.Grb.Abstractions;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Translators;
    using FluentAssertions;
    using Xunit;

    [Collection(ZipArchiveCollectionFixture.COLLECTION)]
    public class ZipArchiveTranslatorTests
    {
        private readonly ZipArchiveFixture _fixture;

        public ZipArchiveTranslatorTests(ZipArchiveFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ExtractingGebouwAllZip_ReturnsJobRecords()
        {
            // Arrange
            var zipArchiveTranslator = new ZipArchiveTranslator(Encoding.UTF8);

            // Act
            var jobRecords = zipArchiveTranslator.Translate(_fixture.ZipArchive).ToList();

            // Assert
            jobRecords.Should().HaveCount(10);

            var jobRecord = jobRecords.First();
            jobRecord.Idn.Should().Be(2857440);
            jobRecord.IdnVersion.Should().Be(1);
            jobRecord.VersionDate.Should().Be(new DateTimeOffset(new DateTime(2011,12,20)));
            jobRecord.EndDate.Should().BeNull();
            jobRecord.EventType.Should().Be(GrbEventType.MeasureBuilding);
            jobRecord.GrbObject.Should().Be(GrbObject.BuildingAtGroundLevel);
            jobRecord.GrId.Should().Be(14207619);
            jobRecord.GrbObjectType.Should().Be(GrbObjectType.OutBuilding);

            jobRecords
                .Should()
                .AllSatisfy(x => x.Geometry.Should().NotBeNull());

            jobRecords
                .Where(x => x.EventType == GrbEventType.DefineBuilding)
                .Should()
                .AllSatisfy(x => x.GrId.Should().Be(-9));

            jobRecords
                .Where(x => x.EventType != GrbEventType.DefineBuilding)
                .Should()
                .AllSatisfy(x => x.GrId.Should().BePositive());

            jobRecords
                .Where(x => x.EventType == GrbEventType.DemolishBuilding)
                .Should()
                .AllSatisfy(x => x.EndDate.Should().NotBeNull());
        }
    }
}

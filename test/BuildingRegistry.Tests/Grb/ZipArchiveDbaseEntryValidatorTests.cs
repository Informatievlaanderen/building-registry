namespace BuildingRegistry.Tests.Grb
{
    using System.Collections.Generic;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Grb.Processor.Upload.Zip;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
    using FluentAssertions;
    using Moq;
    using Xunit;

    [Collection(ZipArchiveCollectionFixture.COLLECTION)]
    public class ZipArchiveDbaseEntryValidatorTests
    {
        private readonly ZipArchiveFixture _fixture;

        private class InvalidSchema : DbaseSchema {}

        public ZipArchiveDbaseEntryValidatorTests(ZipArchiveFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void WithProblems_ReturnsProblems()
        {
            var problems = new Dictionary<RecordNumber, List<ValidationErrorType>>()
            {
                { new RecordNumber(1), new List<ValidationErrorType>() { ValidationErrorType.InvalidGrId } }
            };
            var recordValidatorMock = new Mock<IZipArchiveDbaseRecordsValidator<GrbDbaseRecord>>();
            recordValidatorMock
                .Setup(x => x.Validate(ZipArchiveConstants.DBF_FILENAME, It.IsAny<IEnumerator<GrbDbaseRecord>>()))
                .Returns(problems);

            var sut = new ZipArchiveDbaseEntryValidator<GrbDbaseRecord>(
                Encoding.UTF8,
                new DbaseFileHeaderReadBehavior(true),
                new GrbDbaseSchema(),
                recordValidatorMock.Object);

            // Act
            var validationResult = sut.Validate(_fixture.ZipArchive.GetEntry(ZipArchiveConstants.DBF_FILENAME));

            // Assert
            recordValidatorMock
                .Verify(x => x.Validate(ZipArchiveConstants.DBF_FILENAME, It.IsAny<IEnumerator<GrbDbaseRecord>>()), Times.Once);

            validationResult.Should().BeEquivalentTo(problems);
        }

        [Fact]
        public void WithInvalidSchema_ThrowsDbaseHeaderSchemaMismatchException()
        {
            var sut = new ZipArchiveDbaseEntryValidator<GrbDbaseRecord>(
                Encoding.UTF8,
                new DbaseFileHeaderReadBehavior(true),
                new InvalidSchema(),
                new Mock<IZipArchiveDbaseRecordsValidator<GrbDbaseRecord>>().Object);

            // Act
            var func = () => sut.Validate(_fixture.ZipArchive.GetEntry(ZipArchiveConstants.DBF_FILENAME));

            // Assert
            func.Should().Throw<DbaseHeaderSchemaMismatchException>();
        }
    }
}

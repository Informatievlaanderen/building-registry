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
    public class ZipArchiveShapeEntryValidatorTests
    {
        private readonly ZipArchiveFixture _fixture;

        public ZipArchiveShapeEntryValidatorTests(ZipArchiveFixture fixture)
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
            var shapeValidator = new Mock<IZipArchiveShapeRecordsValidator>();
            shapeValidator
                .Setup(x => x.Validate(ZipArchiveConstants.SHP_FILENAME, It.IsAny<IEnumerator<ShapeRecord>>()))
                .Returns(problems);

            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.UTF8,
                shapeValidator.Object);

            // Act
            var validationResult = sut.Validate(_fixture.ZipArchive.GetEntry(ZipArchiveConstants.SHP_FILENAME));

            // Assert
            shapeValidator
                .Verify(x => x.Validate(ZipArchiveConstants.SHP_FILENAME, It.IsAny<IEnumerator<ShapeRecord>>()), Times.Once);

            validationResult.Should().BeEquivalentTo(problems);
        }

        [Fact]
        public void NoRecords_ThrowsShapeHeaderFormatException()
        {
            var sut = new ZipArchiveShapeEntryValidator(
                Encoding.UTF8,
                new GrbShapeRecordsValidator());

            // Act
            var func = () => sut.Validate(_fixture.ZipArchive.GetEntry(ZipArchiveConstants.DBF_FILENAME));

            // Assert
            func.Should().Throw<ShapeHeaderFormatException>();
        }
    }
}

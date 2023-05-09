namespace BuildingRegistry.Tests.Grb
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
    using FluentAssertions;
    using Xunit;

    public class GrbShapeRecordsValidatorTests
    {
        [Fact]
        public void WithPoint_ThenValidationErrorTypeIsGeometryIsNotPolygon()
        {
            var validator = new GrbShapeRecordsValidator();
            var records = new List<ShapeRecord>()
            {
                new ShapeRecord(new ShapeRecordHeader(new RecordNumber(1), new WordLength(10)),
                    new PointShapeContent(new Point(5.1, 2.2)))
            };

            // Act
            var result = validator.Validate("dummy", records.GetEnumerator());

            // Assert
            var validationErrorTypes = result[new RecordNumber(1)];
            validationErrorTypes.Should().NotBeNullOrEmpty();
            validationErrorTypes.First().Should().Be(ValidationErrorType.GeometryIsNotPolygon);
        }

        [Fact]
        public void WithValidPolygon_ThenNoValidationErrorTypes()
        {
            // Arrange
            var validator = new GrbShapeRecordsValidator();
            var records = new List<ShapeRecord>()
            {
                new ShapeRecord(new ShapeRecordHeader(new RecordNumber(1), new WordLength(10)),
                    new PolygonShapeContent(new Polygon(new BoundingBox2D(1,1,1,1), new []{1}, new Point[0])))
            };

            // Act
            var result = validator.Validate("dummy", records.GetEnumerator());

            // Assert
            result.Should().BeNullOrEmpty();
        }

        [Fact]
        public void WithNoShapeRecords_ThenException()
        {
            // Arrange
            var validator = new GrbShapeRecordsValidator();
            var records = new List<ShapeRecord>();

            // Act
            var func = () => validator.Validate("dummy", records.GetEnumerator());

            // Assert
            func.Should().Throw<NoShapeRecordsException>("dummy");
        }
    }
}

namespace BuildingRegistry.Tests.Grb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using BuildingRegistry.Grb.Processor.Upload;
    using BuildingRegistry.Grb.Processor.Upload.Zip;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Core;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Validators;
    using FluentAssertions;
    using Moq;
    using Xunit;

    [Collection(ZipArchiveCollectionFixture.COLLECTION)]
    public class ZipArchiveValidatorTests
    {
        private readonly ZipArchiveFixture _fixture;

        public ZipArchiveValidatorTests(ZipArchiveFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void WhenMissingFile_ThenValidationError()
        {
            using var zipFile = new FileStream($"{AppContext.BaseDirectory}/Grb/gebouw_dbf_missing.zip", FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(zipFile, ZipArchiveMode.Read, false);

            // Act
            var sut = new ZipArchiveValidator(UploadProcessor.GrbArchiveEntryStructure);
            var zipArchiveProblems = sut.Validate(archive);

            // Assert
            var fileProblem = zipArchiveProblems.FirstOrDefault(x => x is FileError error && error.File == ZipArchiveConstants.DBF_FILENAME.ToUpper());
            fileProblem.Should().NotBeNull();
            fileProblem.Reason.Should().Be("RequiredFileMissing");
        }

        [Fact]
        public void WhenShapeHeaderFormatException_ThenProblem()
        {
            var zipArchiveRecordEntryValidator = new Mock<IZipArchiveDbaseEntryValidator>();
            zipArchiveRecordEntryValidator
                .Setup(x => x.Validate(It.IsAny<ZipArchiveEntry>()))
                .Throws(new ShapeHeaderFormatException(ZipArchiveConstants.DBF_FILENAME, new Exception()));

            // Act
            var sut = new ZipArchiveValidator(new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    ZipArchiveConstants.DBF_FILENAME,
                    zipArchiveRecordEntryValidator.Object
                },
                {
                    ZipArchiveConstants.SHP_FILENAME,
                    new ZipArchiveShapeEntryValidator(Encoding.UTF8, new GrbShapeRecordsValidator())
                }
            });
            var zipArchiveProblems = sut.Validate(_fixture.ZipArchive);

            // Assert
            var fileProblem = zipArchiveProblems.FirstOrDefault(x => x is FileError error && error.File == ZipArchiveConstants.DBF_FILENAME);
            fileProblem.Should().NotBeNull();
            fileProblem.Reason.Should().Be("ShapeHeaderFormatException");
        }

        [Fact]
        public void WhenDbaseHeaderFormatException_ThenProblem()
        {
            var zipArchiveRecordEntryValidator = new Mock<IZipArchiveDbaseEntryValidator>();
            zipArchiveRecordEntryValidator
                .Setup(x => x.Validate(It.IsAny<ZipArchiveEntry>()))
                .Throws(new DbaseHeaderFormatException(ZipArchiveConstants.DBF_FILENAME, new Exception()));

            // Act
            var sut = new ZipArchiveValidator(new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    ZipArchiveConstants.DBF_FILENAME,
                    zipArchiveRecordEntryValidator.Object
                },
                {
                    ZipArchiveConstants.SHP_FILENAME,
                    new ZipArchiveShapeEntryValidator(Encoding.UTF8, new GrbShapeRecordsValidator())
                }
            });
            var zipArchiveProblems = sut.Validate(_fixture.ZipArchive);

            // Assert
            var fileProblem = zipArchiveProblems.FirstOrDefault(x => x is FileError error && error.File == ZipArchiveConstants.DBF_FILENAME);
            fileProblem.Should().NotBeNull();
            fileProblem.Reason.Should().Be("DbaseHeaderFormatException");
        }

        [Fact]
        public void WhenDbaseHeaderSchemaMismatchException_ThenProblem()
        {
            var zipArchiveRecordEntryValidator = new Mock<IZipArchiveDbaseEntryValidator>();
            zipArchiveRecordEntryValidator
                .Setup(x => x.Validate(It.IsAny<ZipArchiveEntry>()))
                .Throws(new DbaseHeaderSchemaMismatchException(ZipArchiveConstants.DBF_FILENAME));

            // Act
            var sut = new ZipArchiveValidator(new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    ZipArchiveConstants.DBF_FILENAME,
                    zipArchiveRecordEntryValidator.Object
                },
                {
                    ZipArchiveConstants.SHP_FILENAME,
                    new ZipArchiveShapeEntryValidator(Encoding.UTF8, new GrbShapeRecordsValidator())
                }
            });
            var zipArchiveProblems = sut.Validate(_fixture.ZipArchive);

            // Assert
            var fileProblem = zipArchiveProblems.FirstOrDefault(x => x is FileError error && error.File == ZipArchiveConstants.DBF_FILENAME);
            fileProblem.Should().NotBeNull();
            fileProblem.Reason.Should().Be("DbaseHeaderSchemaMismatchException");
        }

        [Fact]
        public void WhenNoDbaseRecordsException_ThenProblem()
        {
            var zipArchiveRecordEntryValidator = new Mock<IZipArchiveDbaseEntryValidator>();
            zipArchiveRecordEntryValidator
                .Setup(x => x.Validate(It.IsAny<ZipArchiveEntry>()))
                .Throws(new NoDbaseRecordsException(ZipArchiveConstants.DBF_FILENAME));

            // Act
            var sut = new ZipArchiveValidator(new Dictionary<string, IZipArchiveEntryValidator>(StringComparer.InvariantCultureIgnoreCase)
            {
                {
                    ZipArchiveConstants.DBF_FILENAME,
                    zipArchiveRecordEntryValidator.Object
                },
                {
                    ZipArchiveConstants.SHP_FILENAME,
                    new ZipArchiveShapeEntryValidator(Encoding.UTF8, new GrbShapeRecordsValidator())
                }
            });
            var zipArchiveProblems = sut.Validate(_fixture.ZipArchive);

            // Assert
            var fileProblem = zipArchiveProblems.FirstOrDefault(x => x is FileError error && error.File == ZipArchiveConstants.DBF_FILENAME);
            fileProblem.Should().NotBeNull();
            fileProblem.Reason.Should().Be("NoDbaseRecordsException");
        }
    }
}

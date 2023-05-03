namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Exceptions;

    public class ZipArchiveShapeEntryValidator : IZipArchiveEntryValidator
    {
        private readonly Encoding _encoding;
        private readonly IZipArchiveShapeRecordsValidator _recordValidator;

        public ZipArchiveShapeEntryValidator(Encoding encoding, IZipArchiveShapeRecordsValidator recordValidator)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public IDictionary<RecordNumber, List<ValidationErrorType>> Validate(ZipArchiveEntry entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            var problems = new Dictionary<RecordNumber, List<ValidationErrorType>>();

            using var stream = entry.Open();
            using var reader = new BinaryReader(stream, _encoding);

            ShapeFileHeader? header = null;

            try
            {
                header = ShapeFileHeader.Read(reader);
            }
            catch (Exception exception)
            {
                throw new ShapeHeaderFormatException(exception);
            }

            using var records = header.CreateShapeRecordEnumerator(reader);
            var recordProblems = _recordValidator.Validate(entry, records);

            foreach (var (key, value) in recordProblems)
            {
                problems.Add(key, value);
            }

            return problems;
        }
    }
}

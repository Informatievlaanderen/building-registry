namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveShapeEntryTranslator : IZipArchiveEntryTranslator
    {
        private readonly Encoding _encoding;
        private readonly IZipArchiveShapeRecordsTranslator _shapeRecordsTranslator;

        public ZipArchiveShapeEntryTranslator(Encoding encoding, IZipArchiveShapeRecordsTranslator recordValidator)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _shapeRecordsTranslator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public IDictionary<RecordNumber, JobRecord> Translate(ZipArchiveEntry entry, IDictionary<RecordNumber, JobRecord> jobRecords)
        {
            ArgumentNullException.ThrowIfNull(entry);
            ArgumentNullException.ThrowIfNull(jobRecords);

            using var stream = entry.Open();
            using var reader = new BinaryReader(stream, _encoding);

            var header = ShapeFileHeader.Read(reader);

            using var enumerator = header.CreateShapeRecordEnumerator(reader);
            return _shapeRecordsTranslator.Translate(enumerator, jobRecords);
        }
    }
}

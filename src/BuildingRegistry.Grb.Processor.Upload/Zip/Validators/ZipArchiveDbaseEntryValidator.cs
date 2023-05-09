namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Exceptions;

    public class ZipArchiveDbaseEntryValidator<TDbaseRecord> : IZipArchiveDbaseEntryValidator
        where TDbaseRecord : DbaseRecord, new()
    {
        private readonly IZipArchiveDbaseRecordsValidator<TDbaseRecord> _recordValidator;

        public ZipArchiveDbaseEntryValidator(
            Encoding encoding,
            DbaseFileHeaderReadBehavior readBehavior,
            DbaseSchema schema,
            IZipArchiveDbaseRecordsValidator<TDbaseRecord> recordValidator)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            HeaderReadBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _recordValidator = recordValidator ?? throw new ArgumentNullException(nameof(recordValidator));
        }

        public Encoding Encoding { get; }
        public DbaseFileHeaderReadBehavior HeaderReadBehavior { get; }
        public DbaseSchema Schema { get; }

        public IDictionary<RecordNumber, List<ValidationErrorType>> Validate(ZipArchiveEntry? entry)
        {
            ArgumentNullException.ThrowIfNull(entry);

            var problems = new Dictionary<RecordNumber, List<ValidationErrorType>>();
            using var stream = entry.Open();
            using var reader = new BinaryReader(stream, Encoding);
            DbaseFileHeader? header = null;
            try
            {
                header = DbaseFileHeader.Read(reader, HeaderReadBehavior);
            }
            catch (Exception exception)
            {
                throw new DbaseHeaderFormatException(entry.Name, exception);
            }

            if (!header.Schema.Equals(Schema))
            {
                throw new DbaseHeaderSchemaMismatchException(entry.Name);
            }

            using var records = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader);
            var recordProblems = _recordValidator.Validate(entry.FullName, records);
            foreach (var (key, value) in recordProblems)
            {
                problems.Add(key, value);
            }

            return problems;
        }
    }
}

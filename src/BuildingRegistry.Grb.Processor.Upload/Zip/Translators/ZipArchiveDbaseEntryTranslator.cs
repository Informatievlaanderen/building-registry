namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ZipArchiveDbaseEntryTranslator : IZipArchiveEntryTranslator
    {
        private readonly Encoding _encoding;
        private readonly DbaseFileHeaderReadBehavior _readBehavior;
        private readonly IZipArchiveDbaseRecordsTranslator _translator;

        public ZipArchiveDbaseEntryTranslator(
            Encoding encoding,
            DbaseFileHeaderReadBehavior readBehavior,
            IZipArchiveDbaseRecordsTranslator translator)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _readBehavior = readBehavior ?? throw new ArgumentNullException(nameof(readBehavior));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
        }

        public IDictionary<RecordNumber, JobRecord> Translate(ZipArchiveEntry entry, IDictionary<RecordNumber, JobRecord> _)
        {
            ArgumentNullException.ThrowIfNull(entry);

            using var stream = entry.Open();
            using var reader = new BinaryReader(stream, _encoding);

            var header = DbaseFileHeader.Read(reader, _readBehavior);

            using var enumerator = header.CreateDbaseRecordEnumerator<GrbDbaseRecord>(reader);
            return _translator.Translate(enumerator);
        }
    }
}

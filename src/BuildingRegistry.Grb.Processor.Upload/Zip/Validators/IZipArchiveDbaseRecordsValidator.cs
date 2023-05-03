namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordsValidator<TDbaseRecord>
        where TDbaseRecord : DbaseRecord, new()
    {
        IDictionary<RecordNumber, List<ValidationErrorType>> Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TDbaseRecord> records);
    }
}

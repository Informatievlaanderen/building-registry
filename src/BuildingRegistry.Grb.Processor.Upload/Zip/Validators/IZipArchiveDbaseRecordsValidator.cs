namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordsValidator<in TDbaseRecord>
        where TDbaseRecord : DbaseRecord, new()
    {
        IDictionary<RecordNumber, List<ValidationErrorType>> Validate(string zipArchiveEntryName, IEnumerator<TDbaseRecord> records);
    }
}

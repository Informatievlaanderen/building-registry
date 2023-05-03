namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class GrbDbaseRecordsValidator : IZipArchiveDbaseRecordsValidator<GrbDbaseRecord>
    {
        public GrbDbaseRecordsValidator()
        {

        }

        public IDictionary<RecordNumber, List<ValidationErrorType>> Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<GrbDbaseRecord> records)
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System.Collections.Generic;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveDbaseRecordsTranslator
    {
        IDictionary<RecordNumber, JobRecord> Translate(IDbaseRecordEnumerator<GrbDbaseRecord> records);
    }
}

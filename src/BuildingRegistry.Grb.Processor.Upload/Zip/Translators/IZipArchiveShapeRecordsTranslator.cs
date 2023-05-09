namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System.Collections.Generic;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveShapeRecordsTranslator
    {
        IDictionary<RecordNumber, JobRecord> Translate(IEnumerator<ShapeRecord> records, IDictionary<RecordNumber, JobRecord> jobRecords);
    }
}


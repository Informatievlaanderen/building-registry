namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public interface IZipArchiveShapeRecordsValidator
    {
        IDictionary<RecordNumber, List<ValidationErrorType>> Validate(
            string zipArchiveEntryName,
            IEnumerator<ShapeRecord> records);
    }
}

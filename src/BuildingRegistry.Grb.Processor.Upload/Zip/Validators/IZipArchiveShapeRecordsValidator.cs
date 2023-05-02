namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;

using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IZipArchiveShapeRecordsValidator
{
    ZipArchiveProblems Validate(ZipArchiveEntry entry, IEnumerator<ShapeRecord> records);
}

namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;

using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IZipArchiveDbaseRecordsValidator<TDbaseRecord>
    where TDbaseRecord : DbaseRecord, new()
{
    ZipArchiveProblems Validate(ZipArchiveEntry entry, IDbaseRecordEnumerator<TDbaseRecord> records);
}

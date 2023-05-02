namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;

using System.IO.Compression;

public interface IZipArchiveEntryValidator
{
    ZipArchiveProblems Validate(ZipArchiveEntry entry);
}

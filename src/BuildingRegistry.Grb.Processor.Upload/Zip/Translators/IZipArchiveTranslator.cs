namespace BuildingRegistry.Grb.Processor.Upload.Zip.Translators
{
    using System.Collections.Generic;
    using System.IO.Compression;
    using Abstractions;

    public interface IZipArchiveTranslator
    {
        IEnumerable<JobRecord> Translate(ZipArchive archive);
    }
}

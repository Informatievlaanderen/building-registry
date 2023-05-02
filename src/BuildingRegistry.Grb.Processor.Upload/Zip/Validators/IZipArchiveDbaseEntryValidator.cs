namespace BuildingRegistry.Grb.Processor.Upload.Zip.Validators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;

public interface IZipArchiveDbaseEntryValidator : IZipArchiveEntryValidator
{
    Encoding Encoding { get; }
    DbaseFileHeaderReadBehavior HeaderReadBehavior { get; }
    DbaseSchema Schema { get; }
}

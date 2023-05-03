namespace BuildingRegistry.Grb.Processor.Upload.Zip.ErrorBuilders
{
    public interface IFileRecordProblemBuilder
    {
        IFileErrorBuilder Error(string reason);
        IFileWarningBuilder Warning(string reason);
    }
}

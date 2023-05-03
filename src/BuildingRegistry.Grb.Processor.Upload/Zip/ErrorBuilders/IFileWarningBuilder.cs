namespace BuildingRegistry.Grb.Processor.Upload.Zip.ErrorBuilders
{
    using Core;

    public interface IFileWarningBuilder
    {
        FileWarning Build();
        IFileWarningBuilder WithParameter(ProblemParameter parameter);
        IFileWarningBuilder WithParameters(params ProblemParameter[] parameters);
    }
}
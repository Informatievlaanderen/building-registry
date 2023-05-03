namespace BuildingRegistry.Grb.Processor.Upload.Zip.ErrorBuilders
{
    using Core;

    public interface IFileErrorBuilder
    {
        FileError Build();
        IFileErrorBuilder WithParameter(ProblemParameter parameter);
        IFileErrorBuilder WithParameters(params ProblemParameter[] parameters);
    }
}

namespace BuildingRegistry.Grb.Processor.Upload.Zip.Core
{
    using System.Linq;
    using BuildingRegistry.Grb.Processor.Upload.Zip.Messages;

    public class FileError : FileProblem
    {
        public FileError(string file, string reason, params Core.ProblemParameter[] parameters)
            : base(file, reason, parameters)
        {
        }

        public override Messages.FileProblem Translate()
        {
            return new Messages.FileProblem
            {
                File = File,
                Severity = ProblemSeverity.Error,
                Reason = Reason,
                Parameters = Parameters.Select(parameter => parameter.Translate()).ToArray()
            };
        }
    }
}

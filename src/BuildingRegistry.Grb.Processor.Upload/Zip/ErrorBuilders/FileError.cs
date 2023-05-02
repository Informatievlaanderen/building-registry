namespace BuildingRegistry.Grb.Processor.Upload.Zip.ErrorBuilders;

using System.Linq;
using Messages;
using ProblemParameter = Core.ProblemParameter;

public class FileError : FileProblem
{
    public FileError(string file, string reason, params ProblemParameter[] parameters)
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

namespace BuildingRegistry.Grb.Processor.Upload.Zip.Messages;

using Core;

public class FileProblem
{
    public string File { get; set; }
    public ProblemParameter[] Parameters { get; set; }
    public string Reason { get; set; }
    public ProblemSeverity Severity { get; set; }
}

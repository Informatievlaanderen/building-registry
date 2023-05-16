namespace BuildingRegistry.Grb.Abstractions
{
    public enum JobStatus
    {
        Created = 1,
        Preparing,
        Prepared,
        Processing,
        Completed,
        Cancelled,
        Error
    }
}

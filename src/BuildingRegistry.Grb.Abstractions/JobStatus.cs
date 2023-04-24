namespace BuildingRegistry.Grb.Abstractions
{
    public enum JobStatus
    {
        Unknown = 0,
        Created,
        Preparing,
        Prepared,
        Processing,
        Completed,
        Cancelled,
        Error
    }
}

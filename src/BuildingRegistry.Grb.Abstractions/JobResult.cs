namespace BuildingRegistry.Grb.Abstractions
{
    using System;

    public sealed class JobResult
    {
        public long Id { get; set; }
        public Guid JobId { get; set; }
        public int GrbIdn { get; set; }
        public int BuildingPersistentLocalId { get; set; }
        public bool IsBuildingCreated { get; set; }
    }
}

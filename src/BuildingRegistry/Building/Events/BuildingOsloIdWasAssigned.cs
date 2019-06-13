namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingOsloIdWasAssigned")]
    [EventDescription("Het gebouw kreeg een Oslo Id toegekend.")]
    public class BuildingOsloIdWasAssigned
    {
        public Guid BuildingId { get; }
        public int OsloId { get; }
        public Instant AssignmentDate { get; }

        public BuildingOsloIdWasAssigned(
            BuildingId buildingId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            BuildingId = buildingId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private BuildingOsloIdWasAssigned(
            Guid buildingId,
            int osloId,
            Instant assignmentDate)
            : this(
                new BuildingId(buildingId),
                new OsloId(osloId),
                new OsloAssignmentDate(assignmentDate)) {}
    }
}

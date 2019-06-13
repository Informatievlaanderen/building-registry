namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitOsloIdWasAssigned")]
    [EventDescription("De gebouweenheid kreeg een Oslo Id toegekend.")]
    public class BuildingUnitOsloIdWasAssigned
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public int OsloId { get; }
        public Instant AssignmentDate { get; }

        public BuildingUnitOsloIdWasAssigned(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private BuildingUnitOsloIdWasAssigned(
            Guid buildingId,
            Guid buildingUnitId,
            int osloId,
            Instant assignmentDate)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new OsloId(osloId),
                new OsloAssignmentDate(assignmentDate)) {}
    }
}

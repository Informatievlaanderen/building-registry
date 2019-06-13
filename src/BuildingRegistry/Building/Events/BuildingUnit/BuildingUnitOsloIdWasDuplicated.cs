namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitOsloIdWasDuplicated")]
    [EventDescription("Een gebouweenheid werd een 2e oslo id toegkend door een bug.")]
    public class BuildingUnitOsloIdWasDuplicated
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public int DuplicateOsloId { get; }
        public int OriginalOsloId { get; }
        public Instant DuplicateAssignmentDate { get; }

        public BuildingUnitOsloIdWasDuplicated(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            OsloId duplicateOsloId,
            OsloId originalOsloId,
            OsloAssignmentDate assignmentDate)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            DuplicateOsloId = duplicateOsloId;
            OriginalOsloId = originalOsloId;
            DuplicateAssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private BuildingUnitOsloIdWasDuplicated(
            Guid buildingId,
            Guid buildingUnitId,
            int duplicateOsloId,
            int originalOsloId,
            Instant assignmentDate)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new OsloId(duplicateOsloId),
                new OsloId(originalOsloId),
                new OsloAssignmentDate(assignmentDate)) { }
    }
}

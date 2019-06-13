namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitOsloIdWasRemoved")]
    [EventDescription("De gebouweenheid oslo id werd verwijderd.")]
    public class BuildingUnitOsloIdWasRemoved
    {
        public Guid BuildingId { get; set; }
        public string OsloId { get; set; }
        public Instant AssignmentDate { get; set; }
        public string Reason { get; set; }

        public BuildingUnitOsloIdWasRemoved(
            BuildingId buildingId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate,
            Reason reason)
        {
            BuildingId = buildingId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
            Reason = reason;
        }

        [JsonConstructor]
        private BuildingUnitOsloIdWasRemoved(
            Guid buildingId,
            int osloId,
            Instant assignmentDate,
            string reason)
            : this(
                new BuildingId(buildingId),
                new OsloId(osloId),
                new OsloAssignmentDate(assignmentDate),
                new Reason(reason)) { }
    }
}

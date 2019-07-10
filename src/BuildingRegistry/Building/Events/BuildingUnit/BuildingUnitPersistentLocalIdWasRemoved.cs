namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitPersistentLocalIdentifierWasRemoved")]
    [EventDescription("De persistente lokale id van de gebouweenheid werd verwijderd.")]
    public class BuildingUnitPersistentLocalIdWasRemoved
    {
        public Guid BuildingId { get; set; }
        public string PersistentLocalId { get; set; }
        public Instant AssignmentDate { get; set; }
        public string Reason { get; set; }

        public BuildingUnitPersistentLocalIdWasRemoved(
            BuildingId buildingId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate,
            Reason reason)
        {
            BuildingId = buildingId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
            Reason = reason;
        }

        [JsonConstructor]
        private BuildingUnitPersistentLocalIdWasRemoved(
            Guid buildingId,
            int persistentLocalId,
            Instant assignmentDate,
            string reason)
            : this(
                new BuildingId(buildingId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate),
                new Reason(reason)) { }
    }
}

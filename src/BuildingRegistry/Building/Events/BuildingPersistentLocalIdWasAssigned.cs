namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingPersistentLocalIdentifierWasAssigned")]
    [EventDescription("Het gebouw kreeg een persistente lokale id toegekend.")]
    public class BuildingPersistentLocalIdWasAssigned
    {
        public Guid BuildingId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }

        public BuildingPersistentLocalIdWasAssigned(
            BuildingId buildingId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            BuildingId = buildingId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private BuildingPersistentLocalIdWasAssigned(
            Guid buildingId,
            int persistentLocalId,
            Instant assignmentDate)
            : this(
                new BuildingId(buildingId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate)) {}
    }
}

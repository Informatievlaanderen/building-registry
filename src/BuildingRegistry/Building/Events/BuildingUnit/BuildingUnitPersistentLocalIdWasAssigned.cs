namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitPersistentLocalIdentifierWasAssigned")]
    [EventDescription("De gebouweenheid kreeg een persistente lokale id toegekend.")]
    public class BuildingUnitPersistentLocalIdWasAssigned
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }

        public BuildingUnitPersistentLocalIdWasAssigned(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private BuildingUnitPersistentLocalIdWasAssigned(
            Guid buildingId,
            Guid buildingUnitId,
            int persistentLocalId,
            Instant assignmentDate)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate)) {}
    }
}

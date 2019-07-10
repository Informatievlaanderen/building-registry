namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitPersistentLocalIdentifierWasDuplicated")]
    [EventDescription("Een gebouweenheid werd een 2e persistente lokale id toegekend door een bug.")]
    public class BuildingUnitPersistentLocalIdWasDuplicated
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public int DuplicatePersistentLocalId { get; }
        public int OriginalPersistentLocalId { get; }
        public Instant DuplicateAssignmentDate { get; }

        public BuildingUnitPersistentLocalIdWasDuplicated(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            PersistentLocalId duplicatePersistentLocalId,
            PersistentLocalId originalPersistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            DuplicatePersistentLocalId = duplicatePersistentLocalId;
            OriginalPersistentLocalId = originalPersistentLocalId;
            DuplicateAssignmentDate = assignmentDate;
        }

        [JsonConstructor]
        private BuildingUnitPersistentLocalIdWasDuplicated(
            Guid buildingId,
            Guid buildingUnitId,
            int duplicatePersistentLocalId,
            int originalPersistentLocalId,
            Instant assignmentDate)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new PersistentLocalId(duplicatePersistentLocalId),
                new PersistentLocalId(originalPersistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate)) { }
    }
}

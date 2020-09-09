namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitPersistentLocalIdentifierWasDuplicated")]
    [EventDescription("Een gebouweenheid werd een 2e persistente lokale id toegekend door een bug.")]
    public class BuildingUnitPersistentLocalIdWasDuplicated : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public int DuplicatePersistentLocalId { get; }
        public int OriginalPersistentLocalId { get; }
        public Instant DuplicateAssignmentDate { get; }
        public ProvenanceData Provenance { get; private set; }

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
            Instant assignmentDate,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new PersistentLocalId(duplicatePersistentLocalId),
                new PersistentLocalId(originalPersistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

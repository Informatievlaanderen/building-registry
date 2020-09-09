namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitPersistentLocalIdentifierWasAssigned")]
    [EventDescription("De gebouweenheid kreeg een persistente lokale id toegekend.")]
    public class BuildingUnitPersistentLocalIdWasAssigned : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }
        public ProvenanceData Provenance { get; private set; }

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
            Instant assignmentDate,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

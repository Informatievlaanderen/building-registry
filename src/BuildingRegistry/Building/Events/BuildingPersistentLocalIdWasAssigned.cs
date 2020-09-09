namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingPersistentLocalIdentifierWasAssigned")]
    [EventDescription("Het gebouw kreeg een persistente lokale id toegekend.")]
    public class BuildingPersistentLocalIdWasAssigned : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public int PersistentLocalId { get; }
        public Instant AssignmentDate { get; }
        public ProvenanceData Provenance { get; private set; }

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
            Instant assignmentDate,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

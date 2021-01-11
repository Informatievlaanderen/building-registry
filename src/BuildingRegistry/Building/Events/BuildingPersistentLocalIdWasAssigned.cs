namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingPersistentLocalIdentifierWasAssigned")]
    [EventDescription("Het gebouw kreeg een persistente lokale identificator toegekend.")]
    public class BuildingPersistentLocalIdWasAssigned : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Tijdstip waarop de objectidentificator van het gebouw werd toegekend.")]
        public Instant AssignmentDate { get; }

        [EventPropertyDescription("Metadata bij het event.")]
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

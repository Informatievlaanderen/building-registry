namespace BuildingRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitPersistentLocalIdentifierWasRemoved")]
    [EventDescription("De persistente lokale identificator van de gebouweenheid werd verwijderd.")]
    public class BuildingUnitPersistentLocalIdWasRemoved : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; set; }

        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public string PersistentLocalId { get; set; }

        [EventPropertyDescription("Tijdstip waarop de objectidentificator van de gebouweenheid werd toegekend.")]
        public Instant AssignmentDate { get; set; }

        [EventPropertyDescription("Reden voor het verwijderen van de objectidentificator op de gebouweenheid.")]
        public string Reason { get; set; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; set; }

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
            string reason,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new PersistentLocalId(persistentLocalId),
                new PersistentLocalIdAssignmentDate(assignmentDate),
                new Reason(reason))
            => ((ISetProvenance) this).SetProvenance(provenance.ToProvenance());

        public void SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingStatusWasCorrectedToRemoved")]
    [EventDescription("De gebouwstatus werd verwijderd (via correctie).")]
    public class BuildingStatusWasCorrectedToRemoved : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingStatusWasCorrectedToRemoved(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingStatusWasCorrectedToRemoved(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

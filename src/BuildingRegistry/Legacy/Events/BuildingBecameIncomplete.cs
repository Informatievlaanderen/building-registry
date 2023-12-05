namespace BuildingRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [Obsolete("CRAB events are obsolete.")]
    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingBecameIncomplete")]
    [EventDescription("Het gebouw voldoet niet meer aan het informatiemodel (wegens niet volledig).")]
    public class BuildingBecameIncomplete : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingBecameIncomplete(BuildingId buildingId)
            => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingBecameIncomplete(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

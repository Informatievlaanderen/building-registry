namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingWasCorrectedToUnderConstruction")]
    [EventDescription("Het gebouw kreeg status 'in aanbouw' (via correctie).")]
    public class BuildingWasCorrectedToUnderConstruction : IHasProvenance, ISetProvenance, IMessage
    {
        public BuildingWasCorrectedToUnderConstruction(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingWasCorrectedToUnderConstruction(
            Guid buildingId,
            ProvenanceData provenance)
            : this(new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        void ISetProvenance.SetProvenance(Provenance provenance)
            => Provenance = new ProvenanceData(provenance);
    }
}

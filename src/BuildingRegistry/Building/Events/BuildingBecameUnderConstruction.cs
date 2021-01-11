namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingBecameUnderConstruction")]
    [EventDescription("Het gebouw kreeg status 'in aanbouw'.")]
    public class BuildingBecameUnderConstruction : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingBecameUnderConstruction(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingBecameUnderConstruction(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingGeometryWasRemoved")]
    [EventDescription("De gebouwgeometrie werd verwijderd.")]
    public class BuildingGeometryWasRemoved : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingGeometryWasRemoved(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingGeometryWasRemoved(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

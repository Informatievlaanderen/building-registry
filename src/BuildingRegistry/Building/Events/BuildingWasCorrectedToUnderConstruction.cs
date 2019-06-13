namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasCorrectedToUnderConstruction")]
    [EventDescription("Gebouw werd in aanbouw via correctie")]
    public class BuildingWasCorrectedToUnderConstruction : IHasProvenance, ISetProvenance
    {
        public BuildingWasCorrectedToUnderConstruction(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingWasCorrectedToUnderConstruction(
            Guid buildingId,
            ProvenanceData provenance)
            : this(new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        public Guid BuildingId { get; }
        public ProvenanceData Provenance { get; private set; }

        void ISetProvenance.SetProvenance(Provenance provenance)
            => Provenance = new ProvenanceData(provenance);
    }
}

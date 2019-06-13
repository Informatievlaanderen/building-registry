namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasCorrectedToPlanned")]
    [EventDescription("Gebouw werd gepland via correctie")]
    public class BuildingWasCorrectedToPlanned : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasCorrectedToPlanned(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingWasCorrectedToPlanned(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

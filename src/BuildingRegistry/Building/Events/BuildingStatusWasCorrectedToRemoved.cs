namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingStatusWasCorrectedToRemoved")]
    [EventDescription("Gebouw status werd verwijderd via correctie.")]
    public class BuildingStatusWasCorrectedToRemoved : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
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

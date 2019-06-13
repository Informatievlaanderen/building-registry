namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasPlanned")]
    [EventDescription("Gebouw werd gepland")]
    public class BuildingWasPlanned : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasPlanned(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingWasPlanned(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

namespace BuildingRegistry.Building.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;
    using ValueObjects;

    [EventName("BuildingWasRegistered")]
    [EventDescription("Gebouw werd geregistreerd.")]
    public class BuildingWasRegistered : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasRegistered(BuildingId buildingId) => BuildingId = buildingId;

        [JsonConstructor]
        private BuildingWasRegistered(
            Guid buildingId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

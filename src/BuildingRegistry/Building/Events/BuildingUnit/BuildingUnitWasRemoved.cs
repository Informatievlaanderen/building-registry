namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitWasRemoved")]
    [EventDescription("Gebouweenheid werd verwijderd.")]
    public class BuildingUnitWasRemoved : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasRemoved(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
        }

        [JsonConstructor]
        private BuildingUnitWasRemoved(
            Guid buildingId,
            Guid buildingUnitId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

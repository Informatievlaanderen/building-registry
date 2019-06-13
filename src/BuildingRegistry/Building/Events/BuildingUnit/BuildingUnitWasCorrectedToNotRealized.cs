namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitWasCorrectedToNotRealized")]
    [EventDescription("Gebouweenheid werd niet gerealiseerd via correctie.")]
    public class BuildingUnitWasCorrectedToNotRealized : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }

        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasCorrectedToNotRealized(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
        }

        [JsonConstructor]
        private BuildingUnitWasCorrectedToNotRealized(
            Guid buildingId,
            Guid buildingUnitId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

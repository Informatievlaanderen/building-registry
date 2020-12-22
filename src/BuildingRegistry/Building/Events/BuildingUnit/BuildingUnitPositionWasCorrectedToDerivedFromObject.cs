namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitPositionWasCorrectedToDerivedFromObject")]
    [EventDescription("Gebouweenheid positie werd afgeleid van object.")]
    public class BuildingUnitPositionWasCorrectedToDerivedFromObject : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public string ExtendedWkbGeometry { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitPositionWasCorrectedToDerivedFromObject(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            ExtendedWkbGeometry position)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            ExtendedWkbGeometry = position.ToString();
        }

        [JsonConstructor]
        private BuildingUnitPositionWasCorrectedToDerivedFromObject(
            Guid buildingId,
            Guid buildingUnitId,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new ExtendedWkbGeometry(extendedWkbGeometry)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

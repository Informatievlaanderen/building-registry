namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitWasNotRealizedByParent")]
    [EventDescription(
        "Gebouweenheid werd niet gerealiseerd door een overkoepelende gebouweenheid (bvb. huisnummer > subadres).")]
    public class BuildingUnitWasNotRealizedByParent : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public Guid ParentBuildingUnitId { get; }

        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasNotRealizedByParent(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitId parentBuildingUnitId)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            ParentBuildingUnitId = parentBuildingUnitId;
        }

        [JsonConstructor]
        private BuildingUnitWasNotRealizedByParent(
            Guid buildingId,
            Guid buildingUnitId,
            Guid parentBuildingUnitId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new BuildingUnitId(parentBuildingUnitId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

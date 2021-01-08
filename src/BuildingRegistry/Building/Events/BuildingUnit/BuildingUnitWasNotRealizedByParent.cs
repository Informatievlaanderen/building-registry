namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitWasNotRealizedByParent")]
    [EventDescription("De gebouweenheid kreeg status 'niet gerealiseerd' door een overkoepelende gebouweenheid (bvb. huisnummer > subadres).")]
    public class BuildingUnitWasNotRealizedByParent : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }
        
        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }
        
        [EventPropertyDescription("Interne GUID van de overkoepelende gebouweenheid.")]
        public Guid ParentBuildingUnitId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
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

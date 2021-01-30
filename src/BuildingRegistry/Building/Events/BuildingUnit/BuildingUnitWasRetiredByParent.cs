namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitWasRetiredByParent")]
    [EventDescription("De gebouweenheid kreeg status 'gehistoreerd' door een overkoepelende gebouweenheid (bv. huisnummer > busnummer).")]
    public class BuildingUnitWasRetiredByParent : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }

        [EventPropertyDescription("Interne GUID van de overkoepelende gebouweenheid.")]
        public Guid ParentBuildingUnitId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasRetiredByParent(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitId parentBuildingUnitId)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            ParentBuildingUnitId = parentBuildingUnitId;
        }

        [JsonConstructor]
        private BuildingUnitWasRetiredByParent(
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

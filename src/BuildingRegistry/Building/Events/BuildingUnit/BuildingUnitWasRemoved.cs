namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitWasRemoved")]
    [EventDescription("De gebouweenheid werd verwijderd.")]
    public class BuildingUnitWasRemoved : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
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

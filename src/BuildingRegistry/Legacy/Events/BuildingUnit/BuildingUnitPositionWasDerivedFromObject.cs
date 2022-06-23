namespace BuildingRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitPositionWasDerivedFromObject")]
    [EventDescription("De gebouweenheidpositie werd afgeleid van een ander object.")]
    public class BuildingUnitPositionWasDerivedFromObject : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitPositionWasDerivedFromObject(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            ExtendedWkbGeometry position)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            ExtendedWkbGeometry = position.ToString();
        }

        [JsonConstructor]
        private BuildingUnitPositionWasDerivedFromObject(
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

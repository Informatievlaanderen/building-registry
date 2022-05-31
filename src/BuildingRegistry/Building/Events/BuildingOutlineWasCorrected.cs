namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingOutlineWasCorrected")]
    [EventDescription("Het gebouw werd ingeschetst (via correctie).")]
    public class BuildingOutlineWasCorrected : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingOutlineWasCorrected(
            BuildingId buildingId,
            ExtendedWkbGeometry geometry)
        {
            BuildingId = buildingId;
            ExtendedWkbGeometry = geometry.ToString();
        }

        [JsonConstructor]
        private BuildingOutlineWasCorrected(
            Guid buildingId,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new ExtendedWkbGeometry(extendedWkbGeometry)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasOutlined")]
    [EventDescription("Het gebouw werd ingeschetst.")]
    public class BuildingWasOutlined : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }
        
        [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie.")]
        public string ExtendedWkb { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasOutlined(
            BuildingId buildingId,
            ExtendedWkbGeometry geometry)
        {
            BuildingId = buildingId;
            ExtendedWkb = geometry.ToString();
        }

        [JsonConstructor]
        private BuildingWasOutlined(
            Guid buildingId,
            string extendedWkb,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new ExtendedWkbGeometry(extendedWkb)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

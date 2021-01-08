namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitPositionWasAppointedByAdministrator")]
    [EventDescription("De gebouweenheidpositie werd manueel aangeduid door de beheerder.")]
    public class BuildingUnitPositionWasAppointedByAdministrator : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }
        
        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }
        
        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidgeometrie.")]
        public string Position { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitPositionWasAppointedByAdministrator(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            ExtendedWkbGeometry position)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            Position = position.ToString();
        }

        [JsonConstructor]
        private BuildingUnitPositionWasAppointedByAdministrator(
            Guid buildingId,
            Guid buildingUnitId,
            string position,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new ExtendedWkbGeometry(position)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
        
        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

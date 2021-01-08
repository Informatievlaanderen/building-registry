namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("CommonBuildingUnitWasAdded")]
    [EventDescription("De gebouweenheid met functie 'gemeenschappelijk deel' werd toegevoegd.")]
    public class CommonBuildingUnitWasAdded : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }
        
        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }
        
        [EventPropertyDescription("Informatie in CRAB waarop de aanmaak van de gebouweenheid gebaseerd werd.")]
        public BuildingUnitKeyType BuildingUnitKey { get; }
        
        [EventPropertyDescription("Versie van de gebouweenheid.")]
        public Instant BuildingUnitVersion { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public CommonBuildingUnitWasAdded(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitKey buildingUnitKey,
            BuildingUnitVersion buildingUnitVersion)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            BuildingUnitKey = buildingUnitKey;
            BuildingUnitVersion = buildingUnitVersion;
        }

        [JsonConstructor]
        private CommonBuildingUnitWasAdded(
            Guid buildingId,
            Guid buildingUnitId,
            BuildingUnitKeyType buildingUnitKey,
            Instant buildingUnitVersion,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new BuildingUnitKey(buildingUnitKey),
                new BuildingUnitVersion(buildingUnitVersion)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

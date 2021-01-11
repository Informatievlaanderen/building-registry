namespace BuildingRegistry.Building.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitWasAddedToRetiredBuilding")]
    [EventDescription("De gebouweenheid werd toegevoegd aan een gehistoreerd gebouw.")]
    public class BuildingUnitWasAddedToRetiredBuilding : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }
        
        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }
        
        [EventPropertyDescription("Informatie in CRAB waarop de aanmaak van de gebouweenheid gebaseerd werd.")]
        public BuildingUnitKeyType BuildingUnitKey { get; }
        
        [EventPropertyDescription("Interne GUID van het adres dat aan de gebouweenheid werd gekoppeld.")]
        public Guid AddressId { get; }
        
        [EventPropertyDescription("Versie van de gebouweenheid.")]
        public Instant BuildingUnitVersion { get; }
        
        [EventPropertyDescription("Interne GUID van een eerdere gebouweenheid waarop deze gebouweenheid bij de aanmaak gebaseerd werd.")]
        public Guid? PredecessorBuildingUnitId { get; }
        
        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasAddedToRetiredBuilding(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitKey buildingUnitKey,
            AddressId addressId,
            BuildingUnitVersion buildingUnitVersion)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            BuildingUnitKey = buildingUnitKey;
            AddressId = addressId;
            BuildingUnitVersion = buildingUnitVersion;
        }

        public BuildingUnitWasAddedToRetiredBuilding(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitKey buildingUnitKey,
            AddressId addressId,
            BuildingUnitVersion buildingUnitVersion,
            BuildingUnitId predecessorBuildingUnitId)
            : this(
                buildingId,
                buildingUnitId,
                buildingUnitKey,
                addressId,
                buildingUnitVersion) => PredecessorBuildingUnitId = predecessorBuildingUnitId;

        [JsonConstructor]
        private BuildingUnitWasAddedToRetiredBuilding(
            Guid buildingId,
            Guid buildingUnitId,
            BuildingUnitKeyType buildingUnitKey,
            Guid addressId,
            Instant buildingUnitVersion,
            Guid? predecessorBuildingUnitId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new BuildingUnitKey(buildingUnitKey),
                new AddressId(addressId),
                new BuildingUnitVersion(buildingUnitVersion),
                predecessorBuildingUnitId.HasValue ? new BuildingUnitId(predecessorBuildingUnitId.Value) : null) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
        
        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

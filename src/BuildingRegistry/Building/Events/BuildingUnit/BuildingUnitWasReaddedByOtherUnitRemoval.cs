namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitWasReaddedByOtherUnitRemoval")]
    [EventDescription("De gebouweenheid werd opnieuw toegevoegd door het verwijderen/historeren van een andere gebouweenheid.")]
    public class BuildingUnitWasReaddedByOtherUnitRemoval : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }

        [EventPropertyDescription("Informatie waarop de aanmaak van de gebouweenheid gebaseerd werd.")]
        public BuildingUnitKeyType BuildingUnitKey { get; }

        [EventPropertyDescription("Interne GUID van het adres dat aan de gebouweenheid werd gekoppeld.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Versie van de gebouweenheid.")]
        public Instant BuildingUnitVersion { get; }

        [EventPropertyDescription("Interne GUID van een eerdere gebouweenheid waarop deze gebouweenheid bij de aanmaak gebaseerd werd.")]
        public Guid PredecessorBuildingUnitId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasReaddedByOtherUnitRemoval(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitKey buildingUnitKey,
            AddressId addressId,
            BuildingUnitVersion buildingUnitVersion,
            BuildingUnitId predecessorBuildingUnitId)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            BuildingUnitKey = buildingUnitKey;
            AddressId = addressId;
            BuildingUnitVersion = buildingUnitVersion;
            PredecessorBuildingUnitId = predecessorBuildingUnitId;
        }

        [JsonConstructor]
        private BuildingUnitWasReaddedByOtherUnitRemoval(
            Guid buildingId,
            Guid buildingUnitId,
            BuildingUnitKeyType buildingUnitKey,
            Guid addressId,
            Instant buildingUnitVersion,
            Guid predecessorBuildingUnitId,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new BuildingUnitKey(buildingUnitKey),
                new AddressId(addressId),
                new BuildingUnitVersion(buildingUnitVersion),
                new BuildingUnitId(predecessorBuildingUnitId)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

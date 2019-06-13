namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitWasReaddedByOtherUnitRemoval")]
    [EventDescription(
        "Gebouweenheid werd opnieuw toegevoegd door het verwijderen/historeren van andere gebouweenheid.")]
    public class BuildingUnitWasReaddedByOtherUnitRemoval : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public BuildingUnitKeyType BuildingUnitKey { get; }
        public Guid AddressId { get; }
        public Instant BuildingUnitVersion { get; }
        public Guid PredecessorBuildingUnitId { get; }
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

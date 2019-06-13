namespace BuildingRegistry.Building.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using System;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitWasAddedToRetiredBuilding")]
    [EventDescription("Gebouweenheid werd toegevoegd aan een gehistoreerd gebouw.")]
    public class BuildingUnitWasAddedToRetiredBuilding : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public BuildingUnitKeyType BuildingUnitKey { get; }
        public Guid AddressId { get; }
        public Instant BuildingUnitVersion { get; }
        public Guid? PredecessorBuildingUnitId { get; }
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

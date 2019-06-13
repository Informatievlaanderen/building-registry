namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitWasAdded")]
    [EventDescription("Gebouweenheid werd toegevoegd.")]
    public class BuildingUnitWasAdded : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public BuildingUnitKeyType BuildingUnitKey { get; }
        public Guid AddressId { get; }
        public Instant BuildingUnitVersion { get; }
        public Guid? PredecessorBuildingUnitId { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasAdded(
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

        public BuildingUnitWasAdded(
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
        private BuildingUnitWasAdded(
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
                predecessorBuildingUnitId == null ? null : new BuildingUnitId(predecessorBuildingUnitId.Value)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

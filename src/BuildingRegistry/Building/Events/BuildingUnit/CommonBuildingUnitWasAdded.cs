namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;

    [EventName("CommonBuildingUnitWasAdded")]
    [EventDescription("Gemeenschappelijk gebouweenheid werd toegevoegd.")]
    public class CommonBuildingUnitWasAdded : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public BuildingUnitKeyType BuildingUnitKey { get; }
        public Instant BuildingUnitVersion { get; }
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

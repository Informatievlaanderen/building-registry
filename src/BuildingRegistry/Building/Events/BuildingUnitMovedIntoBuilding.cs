namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("Het gebouweenheid werd verplaatst naar het gebouw.")]
    public sealed class BuildingUnitMovedIntoBuilding : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitMovedIntoBuilding"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van het brongebouw.")]
        public int SourceBuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitMovedIntoBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalId sourceBuildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            SourceBuildingPersistentLocalId = sourceBuildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
        }

        [JsonConstructor]
        private BuildingUnitMovedIntoBuilding(
            int buildingPersistentLocalId,
            int sourceBuildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingPersistentLocalId(sourceBuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(SourceBuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

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
    [EventDescription("De gebouweenheid werd verplaatst.")]
    public class BuildingUnitWasMoved : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitWasMoved"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het doelgebouw.")]
        public int DestinationBuildingPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasMoved(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingPersistentLocalId destinationBuildingPersistentLocalId)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingPersistentLocalId = buildingPersistentLocalId;
            DestinationBuildingPersistentLocalId = destinationBuildingPersistentLocalId;
        }

        [JsonConstructor]
        private BuildingUnitWasMoved(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            int destinationBuildingPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                new BuildingPersistentLocalId(destinationBuildingPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(DestinationBuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

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
    [EventDescription("De afwijkingVastgesteld van de gebouweenheid werd gecorrigeerd naar false.")]
    public sealed class BuildingUnitDeregulationWasCorrected : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitDeregulationWasCorrected"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitDeregulationWasCorrected(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
        }

        [JsonConstructor]
        private BuildingUnitDeregulationWasCorrected(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

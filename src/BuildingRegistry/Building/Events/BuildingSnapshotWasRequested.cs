namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [HideEvent]
    [EventName(EventName)]
    [EventDescription("EventStore snapshot voor het gebouw werd aangevraagd.")]
    public sealed class BuildingSnapshotWasRequested: IBuildingEvent
    {
        public const string EventName = "BuildingSnapshotWasRequested"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingSnapshotWasRequested(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
        }

        [JsonConstructor]
        private BuildingSnapshotWasRequested(int buildingPersistentLocalId, ProvenanceData provenance)
            : this(new BuildingPersistentLocalId(buildingPersistentLocalId))
                => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

namespace BuildingRegistry.AllStream.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using Newtonsoft.Json;

    [EventName(EventName)]
    [EventDescription("Nieuwe OSLO snapshots werd aangevraagd voor de gebouweenheden.")]
    public sealed class BuildingUnitOsloSnapshotsWereRequested : IHasProvenance, ISetProvenance, IMessage
    {
        public const string EventName = "BuildingUnitOsloSnapshotsWereRequested"; // BE CAREFUL CHANGING THIS!!
        
        [EventPropertyDescription("Objectidentificatoren van de gebouweenheden.")]
        public IEnumerable<int> BuildingUnitPersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitOsloSnapshotsWereRequested(
            IEnumerable<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIds)
        {
            BuildingUnitPersistentLocalIds = buildingUnitPersistentLocalIds
                .Select(x => (int)x)
                .ToList();
        }

        [JsonConstructor]
        private BuildingUnitOsloSnapshotsWereRequested(
            IEnumerable<int> buildingUnitPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                buildingUnitPersistentLocalIds.Select(x => new BuildingUnitPersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.AddRange(BuildingUnitPersistentLocalIds.Select(x => x.ToString()));

            return fields;
        }
    }
}

namespace BuildingRegistry.AllStream.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using Newtonsoft.Json;

    [EventName(EventName)]
    [EventDescription("Nieuwe OSLO snapshots werd aangevraagd voor de gebouwen.")]
    public sealed class BuildingOsloSnapshotsWereRequested : IHasProvenance, ISetProvenance, IMessage
    {
        public const string EventName = "BuildingOsloSnapshotsWereRequested"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificatoren van de gebouwen.")]
        public IEnumerable<int> BuildingPersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingOsloSnapshotsWereRequested(
            IEnumerable<BuildingPersistentLocalId> buildingPersistentLocalIds)
        {
            BuildingPersistentLocalIds = buildingPersistentLocalIds
                .Select(x => (int)x)
                .ToList();
        }

        [JsonConstructor]
        private BuildingOsloSnapshotsWereRequested(
            IEnumerable<int> buildingPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                buildingPersistentLocalIds.Select(x => new BuildingPersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.AddRange(BuildingPersistentLocalIds.Select(x => x.ToString()));

            return fields;
        }
    }
}

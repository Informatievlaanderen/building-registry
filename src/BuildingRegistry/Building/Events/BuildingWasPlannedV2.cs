namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ExtendedWkbGeometry = BuildingRegistry.Building.ExtendedWkbGeometry;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit)]
    [EventName(EventName)]
    [EventDescription("Het gebouw werd gepland.")]
    public class BuildingWasPlannedV2 : IBuildingEvent
    {
        public const string EventName = "BuildingWasPlannedV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie.")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasPlannedV2(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            ExtendedWkbGeometry = extendedWkbGeometry.ToString();
        }

        [JsonConstructor]
        private BuildingWasPlannedV2(
            int buildingPersistentLocalId,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new ExtendedWkbGeometry(extendedWkbGeometry))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
        
        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(ExtendedWkbGeometry);

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

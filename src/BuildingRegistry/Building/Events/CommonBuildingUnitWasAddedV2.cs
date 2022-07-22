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
    [EventDescription("Een gebouweenheid gemeenschappelijk deel werd aangemaakt.")]
    public class CommonBuildingUnitWasAddedV2 : IBuildingEvent
    {
        public const string EventName = "CommonBuildingUnitWasAddedV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("De status van de gebouweenheid. Mogelijkheden: Planned of Realized.")]
        public string BuildingUnitStatus { get; set; }

        [EventPropertyDescription("Geometriemethode van de gebouwpositie. Mogelijkheden: Outlined of MeasuredByGrb.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Gebouweenheid afwijking.")]
        public bool HasDeviation { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public CommonBuildingUnitWasAddedV2(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitStatus buildingUnitStatus,
            BuildingUnitPositionGeometryMethod geometryMethod,
            ExtendedWkbGeometry extendedWkbGeometry,
            bool hasDeviation)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingUnitStatus = buildingUnitStatus;
            GeometryMethod = geometryMethod;
            ExtendedWkbGeometry = extendedWkbGeometry;
            HasDeviation = hasDeviation;
        }

        [JsonConstructor]
        private CommonBuildingUnitWasAddedV2(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string buildingUnitStatus,
            string geometryMethod,
            string extendedWkbGeometry,
            bool hasDeviation,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitStatus),
                BuildingUnitPositionGeometryMethod.Parse(geometryMethod),
                new ExtendedWkbGeometry(extendedWkbGeometry),
                hasDeviation)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitStatus);
            fields.Add(GeometryMethod);
            fields.Add(ExtendedWkbGeometry);
            fields.Add(HasDeviation.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

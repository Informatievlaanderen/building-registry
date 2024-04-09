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
    public sealed class BuildingUnitWasMovedIntoBuilding : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitWasMovedIntoBuilding"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van het brongebouw.")]
        public int SourceBuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }
        [EventPropertyDescription("De status van de gebouweenheid. Mogelijkheden: Planned of Realized.")]
        public string BuildingUnitStatus { get; }
        [EventPropertyDescription("Geometriemethode van de gebouweenheidpositie. Mogelijkheden: Outlined of MeasuredByGrb.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Functie van de gebouweenheid.")]
        public string Function { get; }

        [EventPropertyDescription("Gebouweenheid afwijking.")]
        public bool HasDeviation { get; }

        [EventPropertyDescription("Objectidentificatoren van adressen die gekoppeld zijn aan de gebouweenheid.")]
        public List<int> AddressPersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasMovedIntoBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalId sourceBuildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitStatus buildingUnitStatus,
            BuildingUnitPositionGeometryMethod geometryMethod,
            ExtendedWkbGeometry extendedWkbGeometry,
            BuildingUnitFunction function,
            bool hasDeviation,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds
        )
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            SourceBuildingPersistentLocalId = sourceBuildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingUnitStatus = buildingUnitStatus;
            GeometryMethod = geometryMethod;
            ExtendedWkbGeometry = extendedWkbGeometry;
            Function = function;
            HasDeviation = hasDeviation;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(x => (int)x).ToList();
        }

        [JsonConstructor]
        private BuildingUnitWasMovedIntoBuilding(
            int buildingPersistentLocalId,
            int sourceBuildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string buildingUnitStatus,
            string geometryMethod,
            string extendedWkbGeometry,
            string function,
            bool hasDeviation,
            List<int> addressPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingPersistentLocalId(sourceBuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitStatus),
                BuildingUnitPositionGeometryMethod.Parse(geometryMethod),
                new ExtendedWkbGeometry(extendedWkbGeometry),
                BuildingUnitFunction.Parse(function),
                hasDeviation,
                addressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(SourceBuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitStatus);
            fields.Add(GeometryMethod);
            fields.Add(ExtendedWkbGeometry);
            fields.Add(Function);
            fields.Add(HasDeviation.ToString());
            fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

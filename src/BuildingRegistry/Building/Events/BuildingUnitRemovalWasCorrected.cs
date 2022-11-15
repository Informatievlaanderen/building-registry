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
    [EventDescription("De verwijdering van de gebouweenheid werd gecorrigeerd.")]
    public sealed class BuildingUnitRemovalWasCorrected : IBuildingEvent
    {
        public const string EventName = "BuildingUnitRemovalWasCorrected"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("De status van de gebouweenheid. Mogelijkheden: Planned, Realized, NotRealized of Retired.")]
        public string BuildingUnitStatus { get; }

        [EventPropertyDescription("Functie van de gebouweenheid.")]
        public string Function { get; }

        [EventPropertyDescription("Geometriemethode van de gebouweenheidpositie. Mogelijkheden: Outlined of MeasuredByGrb.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Gebouweenheid afwijking.")]
        public bool HasDeviation { get; }

        [EventPropertyDescription("Objectidentificatoren van adressen die gekoppeld zijn aan de gebouweenheid.")]
        public List<int> AddressPersistentLocalIds { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitRemovalWasCorrected(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitStatus buildingUnitStatus,
            BuildingUnitFunction function,
            BuildingUnitPositionGeometryMethod geometryMethod,
            ExtendedWkbGeometry extendedWkbGeometry,
            bool hasDeviation,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingUnitStatus = buildingUnitStatus;
            Function = function;
            GeometryMethod = geometryMethod;
            ExtendedWkbGeometry = extendedWkbGeometry;
            HasDeviation = hasDeviation;
            AddressPersistentLocalIds = addressPersistentLocalIds.Select(x => (int)x).ToList();
        }

        [JsonConstructor]
        private BuildingUnitRemovalWasCorrected(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string buildingUnitStatus,
            string function,
            string geometryMethod,
            string extendedWkbGeometry,
            bool hasDeviation,
            IEnumerable<int> addressPersistentLocalIds,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitStatus),
                BuildingUnitFunction.Parse(function),
                BuildingUnitPositionGeometryMethod.Parse(geometryMethod),
                new ExtendedWkbGeometry(extendedWkbGeometry),
                hasDeviation,
                addressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitStatus);
            fields.Add(Function);
            fields.Add(GeometryMethod);
            fields.Add(ExtendedWkbGeometry);
            fields.Add(HasDeviation.ToString());
            fields.AddRange(AddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

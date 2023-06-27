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
    [EventDescription("De gebouweenheid werd gepland.")]
    public sealed class BuildingUnitWasPlannedV2 : IBuildingEvent
    {
        public const string EventName = "BuildingUnitWasPlannedV2"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Geometriemethode van de gebouweenheidpositie. Mogelijkheden: Outlined of MeasuredByGrb.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Functie van de gebouweenheid.")]
        public string Function { get; }

        [EventPropertyDescription("Gebouweenheid afwijking.")]
        public bool HasDeviation { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasPlannedV2(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod geometryMethod,
            ExtendedWkbGeometry extendedWkbGeometry,
            BuildingUnitFunction function,
            bool hasDeviation)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            GeometryMethod = geometryMethod;
            ExtendedWkbGeometry = extendedWkbGeometry;
            Function = function;
            HasDeviation = hasDeviation;
        }

        [JsonConstructor]
        private BuildingUnitWasPlannedV2(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string geometryMethod,
            string extendedWkbGeometry,
            string function,
            bool hasDeviation,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                BuildingUnitPositionGeometryMethod.Parse(geometryMethod),
                new ExtendedWkbGeometry(extendedWkbGeometry),
                BuildingUnitFunction.Parse(function),
                hasDeviation)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(GeometryMethod);
            fields.Add(ExtendedWkbGeometry);
            fields.Add(Function);
            fields.Add(HasDeviation.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

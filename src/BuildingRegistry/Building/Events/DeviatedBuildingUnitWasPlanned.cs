namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Edit, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("De afwijkende gebouweenheid werd gepland.")]
    public class DeviatedBuildingUnitWasPlanned : IBuildingEvent
    {
        public const string EventName = "DeviatedBuildingUnitWasPlanned"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het afwijkende gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de afwijkende gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Geometriemethode van de gebouwpositie. Mogelijkheden: Outlined of MeasuredByGrb.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Functie van de gebouweenheid.")] // todo: correct description?
        public string Function { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public DeviatedBuildingUnitWasPlanned(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod geometryMethod,
            ExtendedWkbGeometry extendedWkbGeometry,
            BuildingUnitFunction function)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            GeometryMethod = geometryMethod;
            ExtendedWkbGeometry = extendedWkbGeometry;
            Function = function;
        }

        [JsonConstructor]
        private DeviatedBuildingUnitWasPlanned(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string geometryMethod,
            string extendedWkbGeometry,
            string function,
            ProvenanceData provenance)
        : this(
            new BuildingPersistentLocalId(buildingPersistentLocalId),
            new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
            BuildingUnitPositionGeometryMethod.Parse(geometryMethod),
            new ExtendedWkbGeometry(extendedWkbGeometry),
            BuildingUnitFunction.Parse(function))
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
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

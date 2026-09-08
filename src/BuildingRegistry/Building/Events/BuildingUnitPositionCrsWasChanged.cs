namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    /// <summary>
    /// The building unit position was re-expressed in another coordinate reference system without moving:
    /// the one-off transformation of the event store to Lambert 2008 (EPSG 3812), see ADR 0006. Mirrors the
    /// <c>BuildingUnitPositionCrsWasChanged</c> contract in GrAr.Contracts, which is why it restates the
    /// geometry method like <see cref="BuildingUnitPositionWasCorrected"/> even though the transformation
    /// does not change it.
    /// </summary>
    /// <remarks>
    /// Only for units that hold a position of their own. A unit with positieGeometrieMethode
    /// afgeleidVanObject follows the building, so its new position is carried by
    /// <see cref="BuildingGeometryCrsWasChanged"/>.
    /// </remarks>
    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("Het coördinatenreferentiesysteem van de gebouweenheidpositie werd gewijzigd.")]
    public sealed class BuildingUnitPositionCrsWasChanged : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitPositionCrsWasChanged"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Geometriemethode van de gebouweenheidpositie. Mogelijkheden: DerivedFromObject of AppointedByAdministrator.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitPositionCrsWasChanged(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod geometryMethod,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            GeometryMethod = geometryMethod;
            ExtendedWkbGeometry = extendedWkbGeometry;
        }

        [JsonConstructor]
        private BuildingUnitPositionCrsWasChanged(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string geometryMethod,
            string extendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                BuildingUnitPositionGeometryMethod.Parse(geometryMethod),
                new ExtendedWkbGeometry(extendedWkbGeometry))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(GeometryMethod);
            fields.Add(ExtendedWkbGeometry);
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

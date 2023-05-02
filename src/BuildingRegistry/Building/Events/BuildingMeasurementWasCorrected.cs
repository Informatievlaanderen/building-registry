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
    [EventDescription("De geometrie van het ingemeten gebouw werd gecorrigeerd.")]
    public sealed class BuildingMeasurementWasCorrected : IBuildingEvent
    {
        public const string EventName = "BuildingMeasurementWasCorrected"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van de gekoppelde gebouweenheden met positieGeometrieMethode afgeleidVanObject.")]
        public IEnumerable<int> BuildingUnitPersistentLocalIds { get; }

        [EventPropertyDescription("Objectidentificatoren van de gekoppelde gebouweenheden met positieGeometrieMethode aangeduidDoorBeheerder die gewijzigd zijn naar positieGeometrieMethode afgeleidVanObject.")]
        public IEnumerable<int> BuildingUnitPersistentLocalIdsWhichBecameDerived { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometryBuilding { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie van de gekoppelde gebouweenheden met positieGeometrieMethode afgeleidVanObject (Hexadecimale notatie).")]
        public string? ExtendedWkbGeometryBuildingUnits { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingMeasurementWasCorrected(
            BuildingPersistentLocalId buildingPersistentLocalId,
            IEnumerable<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIds,
            IEnumerable<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIdsWhichBecameDerived,
            ExtendedWkbGeometry extendedWkbGeometryBuilding,
            ExtendedWkbGeometry? extendedWkbGeometryBuildingUnits)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalIds = buildingUnitPersistentLocalIds.Select(x => (int)x).ToList();
            BuildingUnitPersistentLocalIdsWhichBecameDerived = buildingUnitPersistentLocalIdsWhichBecameDerived.Select(x => (int)x).ToList();
            ExtendedWkbGeometryBuilding = extendedWkbGeometryBuilding;
            ExtendedWkbGeometryBuildingUnits = extendedWkbGeometryBuildingUnits ?? (string?)null;
        }

        [JsonConstructor]
        private BuildingMeasurementWasCorrected(
            int buildingPersistentLocalId,
            IEnumerable<int> buildingUnitPersistentLocalIds,
            IEnumerable<int> buildingUnitPersistentLocalIdsWhichBecameDerived,
            string extendedWkbGeometryBuilding,
            string? extendedWkbGeometryBuildingUnits,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                buildingUnitPersistentLocalIds.Select(x => new BuildingUnitPersistentLocalId(x)),
                buildingUnitPersistentLocalIdsWhichBecameDerived.Select(x => new BuildingUnitPersistentLocalId(x)),
                new ExtendedWkbGeometry(extendedWkbGeometryBuilding),
                !string.IsNullOrWhiteSpace(extendedWkbGeometryBuildingUnits) ? new ExtendedWkbGeometry(extendedWkbGeometryBuildingUnits) : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.AddRange(BuildingUnitPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            fields.AddRange(BuildingUnitPersistentLocalIdsWhichBecameDerived.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            fields.Add(ExtendedWkbGeometryBuilding);
            if (!string.IsNullOrWhiteSpace(ExtendedWkbGeometryBuildingUnits))
            {
                fields.Add(ExtendedWkbGeometryBuildingUnits);
            }
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

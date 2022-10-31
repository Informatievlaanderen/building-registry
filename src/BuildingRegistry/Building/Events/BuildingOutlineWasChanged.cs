namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("Het gebouw werd ingeschetst (via wijziging).")]
    public class BuildingOutlineWasChanged : IBuildingEvent
    {
        public const string EventName = "BuildingOutlineWasChanged"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van de gekoppelde gebouweenheden met positieGeometrieMethode afgeleidVanObject.")]
        public IEnumerable<int> BuildingUnitPersistentLocalIds { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometryBuilding { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie van de gekoppelde gebouweenheden met positieGeometrieMethode afgeleidVanObject (Hexadecimale notatie).")]
        public string? ExtendedWkbGeometryBuildingUnits { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingOutlineWasChanged(
            BuildingPersistentLocalId buildingPersistentLocalId,
            IEnumerable<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIds,
            ExtendedWkbGeometry extendedWkbGeometryBuilding,
            ExtendedWkbGeometry? extendedWkbGeometryBuildingUnits)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalIds = buildingUnitPersistentLocalIds.Select(x => (int)x).ToList();
            ExtendedWkbGeometryBuilding = extendedWkbGeometryBuilding;

            if (BuildingUnitPersistentLocalIds.Any() && extendedWkbGeometryBuilding is null)
            {
                throw new ArgumentNullException(nameof(extendedWkbGeometryBuildingUnits));
            }

            ExtendedWkbGeometryBuildingUnits = extendedWkbGeometryBuildingUnits ?? (string?)null;
        }

        [JsonConstructor]
        private BuildingOutlineWasChanged(
            int buildingPersistentLocalId,
            IEnumerable<int> buildingUnitPersistentLocalIds,
            string extendedWkbGeometryBuilding,
            string extendedWkbGeometryBuildingUnits,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                buildingUnitPersistentLocalIds.Select(x => new BuildingUnitPersistentLocalId(x)),
                new ExtendedWkbGeometry(extendedWkbGeometryBuilding),
                !string.IsNullOrWhiteSpace(extendedWkbGeometryBuildingUnits)
                    ? new ExtendedWkbGeometry(extendedWkbGeometryBuildingUnits)
                    : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.AddRange(BuildingUnitPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
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

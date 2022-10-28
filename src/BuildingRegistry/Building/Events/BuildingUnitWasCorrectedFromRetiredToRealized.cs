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
    [EventDescription("De gebouweenheid met status gehistoreerd werd gecorrigeerd naar status gerealiseerd.")]
    public class BuildingUnitWasCorrectedFromRetiredToRealized : IBuildingEvent
    {
        public const string EventName = "BuildingUnitWasCorrectedFromRetiredToRealized"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
        public string? DerivedExtendedWkbGeometry { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasCorrectedFromRetiredToRealized(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            ExtendedWkbGeometry? derivedExtendedWkbGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            DerivedExtendedWkbGeometry = derivedExtendedWkbGeometry ?? (string?)null;
        }

        [JsonConstructor]
        private BuildingUnitWasCorrectedFromRetiredToRealized(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            string? derivedExtendedWkbGeometry,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                !string.IsNullOrWhiteSpace(derivedExtendedWkbGeometry)
                    ? new ExtendedWkbGeometry(derivedExtendedWkbGeometry)
                    : null)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(DerivedExtendedWkbGeometry))
            {
                fields.Add(DerivedExtendedWkbGeometry);
            }
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;

    [EventTags(EventTag.For.Sync, Tag.Migration)]
    [EventName(EventName)]
    [EventDescription("Het gebouw werd gemigreerd.")]
    public sealed class BuildingWasMigrated : IBuildingEvent
    {
        public const string EventName = "BuildingWasMigrated"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("Tijdstip waarop de objectidentificator van het gebouw werd toegekend.")]
        public Instant BuildingPersistentLocalIdAssignmentDate { get; }

        [EventPropertyDescription("De status van het gebouw. Mogelijkheden: Planned, UnderConstruction, Realized, Retired en NotRealized.")]
        public string BuildingStatus { get; }

        [EventPropertyDescription("Geometriemethode van de gebouwpositie. Mogelijkheden: Outlined of MeasuredByGrb.")]
        public string GeometryMethod { get; }

        [EventPropertyDescription("Extended WKB-voorstelling van de gebouwgeometrie (Hexadecimale notatie).")]
        public string ExtendedWkbGeometry { get; }

        [EventPropertyDescription("False wanneer het gebouw niet werd verwijderd. True wanneer het gebouw werd verwijderd.")]
        public bool IsRemoved { get; }

        [EventPropertyDescription("De gekoppelde gebouweenheden voor het gebouw.")]
        public List<BuildingUnit> BuildingUnits { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasMigrated(
            BuildingId buildingId,
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalIdAssignmentDate buildingPersistentLocalIdAssignmentDate,
            BuildingStatus buildingStatus,
            BuildingGeometry buildingGeometry,
            bool isRemoved,
            List<Commands.BuildingUnit> buildingUnits)
        {
            BuildingId = buildingId;
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingPersistentLocalIdAssignmentDate = buildingPersistentLocalIdAssignmentDate;
            BuildingStatus = buildingStatus.Value;
            GeometryMethod = buildingGeometry.Method;
            ExtendedWkbGeometry = buildingGeometry.Geometry;
            IsRemoved = isRemoved;
            BuildingUnits = buildingUnits.ConvertAll(x => new BuildingUnit(x));
        }

        [JsonConstructor]
        private BuildingWasMigrated(
            Guid buildingId,
            int buildingPersistentLocalId,
            Instant buildingPersistentLocalIdAssignmentDate,
            string buildingStatus,
            string geometryMethod,
            string extendedWkbGeometry,
            bool isRemoved,
            List<BuildingUnit> buildingUnits,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingPersistentLocalIdAssignmentDate(buildingPersistentLocalIdAssignmentDate),
                BuildingRegistry.Building.BuildingStatus.Parse(buildingStatus),
                new BuildingGeometry(
                    new ExtendedWkbGeometry(extendedWkbGeometry),
                    BuildingGeometryMethod.Parse(geometryMethod)),
                isRemoved,
                new List<Commands.BuildingUnit>())
        {
            BuildingUnits = buildingUnits;
            ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());
        }

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingId.ToString("D"));
            fields.Add(BuildingPersistentLocalId.ToString());
            fields.Add(BuildingPersistentLocalIdAssignmentDate.ToString());
            fields.Add(BuildingStatus);
            fields.Add(GeometryMethod);
            fields.Add(ExtendedWkbGeometry);
            fields.Add(IsRemoved.ToString());
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);

        public sealed class BuildingUnit
        {
            [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
            public Guid BuildingUnitId { get; }

            [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
            public int BuildingUnitPersistentLocalId { get; }

            [EventPropertyDescription("Functie van de gebouweenheid. Mogelijkheden: Common of Unknown.")]
            public string Function { get; }

            [EventPropertyDescription("Status van de gebouweenheid. Mogelijkheden: Planned, Realized, Retired of NotRealized.")]
            public string Status { get; }

            [EventPropertyDescription("Objectidentificatoren van adressen die gekoppeld zijn aan de gebouweenheid.")]
            public List<int> AddressPersistentLocalIds { get; }

            [EventPropertyDescription("Geometriemethode van de gebouweenheid. Mogelijkheden: AppointedByAdministrator of DerivedFromObject.")]
            public string GeometryMethod { get; }

            [EventPropertyDescription("Extended WKB-voorstelling van de gebouweenheidpositie (Hexadecimale notatie).")]
            public string ExtendedWkbGeometry { get; }

            [EventPropertyDescription("False wanneer de gebouweenheid niet werd verwijderd. True wanneer de gebouweenheid werd verwijderd.")]
            public bool IsRemoved { get; }

            public BuildingUnit(Commands.BuildingUnit buildingUnit)
            {
                BuildingUnitId = buildingUnit.BuildingUnitId;
                BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId;
                Function = buildingUnit.Function;
                Status = buildingUnit.Status;
                AddressPersistentLocalIds = buildingUnit.AddressPersistentLocalIds.Select(x => (int)x).ToList();
                GeometryMethod = buildingUnit.BuildingUnitPosition.GeometryMethod;
                ExtendedWkbGeometry = buildingUnit.BuildingUnitPosition.Geometry;
                IsRemoved = buildingUnit.IsRemoved;
            }

            [JsonConstructor]
            private BuildingUnit(
                Guid buildingUnitId,
                int buildingUnitPersistentLocalId,
                string function,
                string status,
                List<int> addressPersistentLocalIds,
                string geometryMethod,
                string extendedWkbGeometry,
                bool isRemoved)
            {
                BuildingUnitId = buildingUnitId;
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
                Function = function;
                Status = status;
                AddressPersistentLocalIds = addressPersistentLocalIds;
                GeometryMethod = geometryMethod;
                ExtendedWkbGeometry = extendedWkbGeometry;
                IsRemoved = isRemoved;
            }
        }
    }
}

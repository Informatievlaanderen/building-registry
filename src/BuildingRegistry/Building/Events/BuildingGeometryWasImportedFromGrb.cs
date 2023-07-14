namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Datastructures;
    using Newtonsoft.Json;
    using NodaTime;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("Het gebouw werd geïmporteerd door GRB.")]
    public sealed class BuildingGeometryWasImportedFromGrb : IBuildingEvent
    {
        public const string EventName = "BuildingGeometryWasImportedFromGrb"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("De gebouw identificator van het GRB.")]
        public long Idn { get; }

        [EventPropertyDescription("De versiedatum van het gebouw.")]
        public Instant VersionDate { get; }

        [EventPropertyDescription("De einddatum van het GRB gebouw, indien geen einddatum dan is dit veld leeg.")]
        public Instant? EndDate { get; }

        [EventPropertyDescription("Versie van de gebouw identificator van het GRB.")]
        public int IdnVersion { get; }

        [EventPropertyDescription("Het soort GRB object.")]
        public string GrbObject { get; }

        [EventPropertyDescription("Type afhankelijk van GrbObject.")]
        public string GrbObjectType { get; }

        [EventPropertyDescription("Het soort event afkomstig van het GRB.")]
        public string EventType { get; }

        [EventPropertyDescription("De geometrie van het gebouw afkomstig van het GRB.")]
        public string Geometry { get; }

        [EventPropertyDescription("Percentage van overlap welke door de GRB matching werd gedetecteerd, indien geen overlap dan is dit veld leeg.")]
        public decimal? Overlap { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingGeometryWasImportedFromGrb(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingGrbData buildingGrbData)
            : this(buildingPersistentLocalId,
                buildingGrbData.Idn,
                buildingGrbData.VersionDate,
                buildingGrbData.EndDate,
                buildingGrbData.IdnVersion,
                buildingGrbData.GrbObject,
                buildingGrbData.GrbObjectType,
                buildingGrbData.EventType,
                buildingGrbData.Geometry,
                buildingGrbData.Overlap)
        { }

        public BuildingGeometryWasImportedFromGrb(
            BuildingPersistentLocalId buildingPersistentLocalId,
            long idn,
            Instant versionDate,
            Instant? endDate,
            int idnVersion,
            string grbObject,
            string grbObjectType,
            string eventType,
            string geometry,
            decimal? overlap)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Idn = idn;
            VersionDate = versionDate;
            EndDate = endDate;
            IdnVersion = idnVersion;
            GrbObject = grbObject;
            GrbObjectType = grbObjectType;
            EventType = eventType;
            Geometry = geometry;
            Overlap = overlap;
        }

        [JsonConstructor]
        private BuildingGeometryWasImportedFromGrb(
            int buildingPersistentLocalId,
            long idn,
            Instant versionDate,
            Instant? endDate,
            int idnVersion,
            string grbObject,
            string grbObjectType,
            string eventType,
            string geometry,
            decimal? overlap,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                idn,
                versionDate,
                endDate,
                idnVersion,
                grbObject,
                grbObjectType,
                eventType,
                geometry,
                overlap)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(Idn.ToString());
            fields.Add(VersionDate.ToString());
            fields.Add(EndDate?.ToString() ?? string.Empty);
            fields.Add(IdnVersion.ToString());
            fields.Add(GrbObject);
            fields.Add(GrbObjectType);
            fields.Add(EventType);
            fields.Add(Geometry);
            fields.Add(Overlap?.ToString() ?? string.Empty);

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

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
    [EventDescription("Het adres werd herkoppeld van de gebouweenheid in functie van een gemeentefusie.")]
    public sealed class BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van het oude gekoppelde adres.")]
        public int OldAddressPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van het nieuwe gekoppelde adres.")]
        public int NewAddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId oldAddressPersistentLocalId,
            AddressPersistentLocalId newAddressPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            OldAddressPersistentLocalId = oldAddressPersistentLocalId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
        }

        [JsonConstructor]
        private BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            int oldAddressPersistentLocalId,
            int newAddressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                new AddressPersistentLocalId(oldAddressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(OldAddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(NewAddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

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
    [EventDescription("Het adres werd ontkoppeld van de gebouweenheid door heradressering.")]
    public sealed class BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed : IBuildingEvent, IHasBuildingUnitPersistentLocalId
    {
        public const string EventName = "BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van het oude gekoppelde adres.")]
        public int PreviousAddressPersistentLocalId { get; }
        [EventPropertyDescription("Objectidentificator van het nieuwe gekoppelde adres.")]
        public int NewAddressPersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId,
            AddressPersistentLocalId newAddressPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            PreviousAddressPersistentLocalId = previousAddressPersistentLocalId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
        }

        [JsonConstructor]
        private BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            int previousAddressPersistentLocalId,
            int newAddressPersistentLocalId,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                new AddressPersistentLocalId(previousAddressPersistentLocalId),
                new AddressPersistentLocalId(newAddressPersistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(PreviousAddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            fields.Add(NewAddressPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }
}

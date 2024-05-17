namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync, EventTag.For.Edit, Tag.Building)]
    [EventName(EventName)]
    [EventDescription("De adresherkoppelingen op de gebouweenheden van het gebouw door heradressering.")]
    public sealed class BuildingBuildingUnitsAddressesWereReaddressed : IBuildingEvent
    {
        public const string EventName = "BuildingBuildingUnitsAddressesWereReaddressed"; // BE CAREFUL CHANGING THIS!!

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int BuildingPersistentLocalId { get; }

        [EventPropertyDescription("De adresherkoppelingen per gebouweenheid.")]
        public IEnumerable<BuildingUnitAddressesWereReaddressed> BuildingUnitsReaddresses { get; }

        [EventPropertyDescription("De geheradresseerde adressen uit het Adressenregister.")]
        public IEnumerable<AddressRegistryReaddress> AddressRegistryReaddresses { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingBuildingUnitsAddressesWereReaddressed(
            BuildingPersistentLocalId buildingPersistentLocalId,
            IEnumerable<BuildingUnitAddressesWereReaddressed> buildingUnitsReaddresses,
            IEnumerable<AddressRegistryReaddress> addressRegistryReaddresses)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitsReaddresses = buildingUnitsReaddresses.ToList();
            AddressRegistryReaddresses = addressRegistryReaddresses.ToList();
        }

        [JsonConstructor]
        private BuildingBuildingUnitsAddressesWereReaddressed(
            int buildingPersistentLocalId,
            IEnumerable<BuildingUnitAddressesWereReaddressed> buildingUnitsReaddresses,
            IEnumerable<AddressRegistryReaddress> addressRegistryReaddresses,
            ProvenanceData provenance)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                buildingUnitsReaddresses,
                addressRegistryReaddresses)
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);

        public IEnumerable<string> GetHashFields()
        {
            var fields = Provenance.GetHashFields().ToList();
            fields.Add(BuildingPersistentLocalId.ToString(CultureInfo.InvariantCulture));
            foreach (var buildingUnitReaddress in BuildingUnitsReaddresses)
            {
                fields.Add(buildingUnitReaddress.BuildingUnitPersistentLocalId.ToString(CultureInfo.InvariantCulture));
                fields.AddRange(buildingUnitReaddress.AttachedAddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
                fields.AddRange(buildingUnitReaddress.DetachedAddressPersistentLocalIds.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            }

            return fields;
        }

        public string GetHash() => this.ToEventHash(EventName);
    }

    public sealed class BuildingUnitAddressesWereReaddressed : IHasBuildingUnitPersistentLocalId
    {
        [EventPropertyDescription("Objectidentificator van de gebouweenheid.")]
        public int BuildingUnitPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificatoren van nieuw gekoppelde adressen.")]
        public IEnumerable<int> AttachedAddressPersistentLocalIds { get; }

        [EventPropertyDescription("Objectidentificatoren van ontkoppelde adressen.")]
        public IEnumerable<int> DetachedAddressPersistentLocalIds { get; }

        public BuildingUnitAddressesWereReaddressed(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            IEnumerable<AddressPersistentLocalId> attachedAddressPersistentLocalIds,
            IEnumerable<AddressPersistentLocalId> detachedAddressPersistentLocalIds)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            AttachedAddressPersistentLocalIds = attachedAddressPersistentLocalIds
                .Select(x => (int)x)
                .ToList();
            DetachedAddressPersistentLocalIds = detachedAddressPersistentLocalIds
                .Select(x => (int)x)
                .ToList();
        }

        [JsonConstructor]
        private BuildingUnitAddressesWereReaddressed(
            int buildingUnitPersistentLocalId,
            IEnumerable<int> attachedAddressPersistentLocalIds,
            IEnumerable<int> detachedAddressPersistentLocalIds)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            AttachedAddressPersistentLocalIds = attachedAddressPersistentLocalIds;
            DetachedAddressPersistentLocalIds = detachedAddressPersistentLocalIds;
        }
    }

    public sealed class AddressRegistryReaddress
    {
        [EventPropertyDescription("Objectidentificator van het bronadres.")]
        public int SourceAddressPersistentLocalId { get; }

        [EventPropertyDescription("Objectidentificator van het doeladres.")]
        public int DestinationAddressPersistentLocalId { get; }

        public AddressRegistryReaddress(
            ReaddressData readdressData)
        {
            SourceAddressPersistentLocalId = readdressData.SourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = readdressData.DestinationAddressPersistentLocalId;
        }

        [JsonConstructor]
        private AddressRegistryReaddress(
            int sourceAddressPersistentLocalId,
            int destinationAddressPersistentLocalId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
        }
    }
}

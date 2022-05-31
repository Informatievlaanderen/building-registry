namespace BuildingRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitAddressWasAttached")]
    [EventDescription("Er werd een adres gekoppeld aan de gebouweenheid.")]
    public class BuildingUnitAddressWasAttached : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van het adres dat aan de gebouweenheid werd gekoppeld.")]
        public Guid AddressId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheid waaraan het adres werd gekoppeld.")]
        public Guid To { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitAddressWasAttached(
            BuildingId buildingId,
            AddressId addressId,
            BuildingUnitId to)
        {
            BuildingId = buildingId;
            AddressId = addressId;
            To = to;
        }

        [JsonConstructor]
        private BuildingUnitAddressWasAttached(
            Guid buildingId,
            Guid addressId,
            Guid to,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new AddressId(addressId),
                new BuildingUnitId(to)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

namespace BuildingRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using NodaTime;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingUnitWasReaddressed")]
    [EventDescription("De gebouweenheid werd geheradresseerd.")]
    public class BuildingUnitWasReaddressed : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw waartoe de gebouweenheid behoort.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheid.")]
        public Guid BuildingUnitId { get; }

        [EventPropertyDescription("Interne GUID van het adres dat vóór de heradressering aan de gebouweenheid gekoppeld was.")]
        public Guid OldAddressId { get; }

        [EventPropertyDescription("Interne GUID van het adres dat na de heradressering aan de gebouweenheid gekoppeld is.")]
        public Guid NewAddressId { get; }

        [EventPropertyDescription("Datum van de heradressering.")]
        public LocalDate BeginDate { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitWasReaddressed(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            AddressId oldAddressId,
            AddressId newAddressId,
            ReaddressingBeginDate beginDate)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            OldAddressId = oldAddressId;
            NewAddressId = newAddressId;
            BeginDate = beginDate;
        }

        [JsonConstructor]
        private BuildingUnitWasReaddressed(
            Guid buildingId,
            Guid buildingUnitId,
            Guid oldAddressId,
            Guid newAddressId,
            LocalDate beginDate,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new AddressId(oldAddressId),
                new AddressId(newAddressId),
                new ReaddressingBeginDate(beginDate))
                    => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

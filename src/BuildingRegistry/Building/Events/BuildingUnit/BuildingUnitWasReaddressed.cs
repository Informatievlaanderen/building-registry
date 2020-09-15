namespace BuildingRegistry.Building.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitWasReaddressed")]
    [EventDescription("Gebouweenheid werd geheradresseerd")]
    public class BuildingUnitWasReaddressed : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public Guid OldAddressId { get; }
        public Guid NewAddressId { get; }
        public LocalDate BeginDate { get; }

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

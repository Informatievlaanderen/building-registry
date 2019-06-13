namespace BuildingRegistry.Building.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitAddressWasAttached")]
    [EventDescription("Een adres werd gekoppeld aan een gebouweenheid")]
    public class BuildingUnitAddressWasAttached : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public Guid AddressId { get; }
        public Guid To { get; }

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

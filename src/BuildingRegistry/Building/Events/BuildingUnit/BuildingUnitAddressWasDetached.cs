namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingUnitAddressWasDetached")]
    [EventDescription("Een adres werd losgekoppeld van een gebouweenheid")]
    public class BuildingUnitAddressWasDetached : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public List<Guid> AddressIds { get; }
        public Guid From { get; }

        public ProvenanceData Provenance { get; private set; }

        public BuildingUnitAddressWasDetached(
            BuildingId buildingId,
            AddressId addressId,
            BuildingUnitId from)
            : this(buildingId, new List<AddressId> { addressId }, from) { }

        public BuildingUnitAddressWasDetached(
            BuildingId buildingId,
            IEnumerable<AddressId> addressIds,
            BuildingUnitId from)
        {
            BuildingId = buildingId;
            AddressIds = addressIds.Select(x => (Guid)x).ToList();
            From = from;
        }

        [JsonConstructor]
        private BuildingUnitAddressWasDetached(
            Guid buildingId,
            IEnumerable<Guid> addressIds,
            Guid from,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                addressIds.Select(x => new AddressId(x)),
                new BuildingUnitId(from)) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

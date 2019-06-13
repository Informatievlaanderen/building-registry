namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasNotRealized")]
    [EventDescription("Gebouw werd niet gerealiseerd")]
    public class BuildingWasNotRealized : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public List<Guid> BuildingUnitIdsToRetire { get; }
        public List<Guid> BuildingUnitIdsToNotRealize { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasNotRealized(
            BuildingId buildingId,
            IEnumerable<BuildingUnitId> buildingUnitIdsToRetire,
            IEnumerable<BuildingUnitId> buildingUnitIdsToNotRealize)
        {
            BuildingId = buildingId;
            BuildingUnitIdsToRetire = buildingUnitIdsToRetire?.Select(x => (Guid)x).ToList();
            BuildingUnitIdsToNotRealize = buildingUnitIdsToNotRealize?.Select(x => (Guid)x).ToList();
        }

        [JsonConstructor]
        private BuildingWasNotRealized(
            Guid buildingId,
            IEnumerable<Guid> buildingUnitIdsToRetire,
            IEnumerable<Guid> buildingUnitIdsToNotRealize,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                buildingUnitIdsToRetire?.Select(x => new BuildingUnitId(x)) ?? new List<BuildingUnitId>(),
                buildingUnitIdsToNotRealize?.Select(x => new BuildingUnitId(x)) ?? new List<BuildingUnitId>()) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

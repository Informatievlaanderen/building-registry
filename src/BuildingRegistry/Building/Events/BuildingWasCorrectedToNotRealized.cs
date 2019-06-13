namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasCorrectedToNotRealized")]
    [EventDescription("Gebouw werd niet gerealiseerd via correctie")]
    public class BuildingWasCorrectedToNotRealized : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public List<Guid> BuildingUnitIdsToRetire { get; }
        public List<Guid> BuildingUnitIdsToNotRealize { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasCorrectedToNotRealized(
            BuildingId buildingId,
            IEnumerable<BuildingUnitId> buildingUnitsToRetire,
            IEnumerable<BuildingUnitId> buildingUnitsToNotRealize)
        {
            BuildingId = buildingId;

            BuildingUnitIdsToRetire = buildingUnitsToRetire?.Select(x => (Guid) x).ToList() ?? new List<Guid>();
            BuildingUnitIdsToNotRealize = buildingUnitsToNotRealize?.Select(x => (Guid) x).ToList() ?? new List<Guid>();
        }

        [JsonConstructor]
        private BuildingWasCorrectedToNotRealized(
            Guid buildingId,
            IEnumerable<Guid> buildingUnitsToRetire,
            IEnumerable<Guid> buildingUnitsToNotRealize,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                buildingUnitsToRetire?.Select(x => new BuildingUnitId(x)) ?? new List<BuildingUnitId>(),
                buildingUnitsToNotRealize?.Select(x => new BuildingUnitId(x)) ?? new List<BuildingUnitId>()) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

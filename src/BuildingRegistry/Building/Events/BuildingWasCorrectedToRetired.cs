namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasCorrectedToRetired")]
    [EventDescription("Het gebouw kreeg status 'gehistoreerd' (via correctie).")]
    public class BuildingWasCorrectedToRetired : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheden die status 'gehistoreerd' moeten krijgen.")]
        public List<Guid> BuildingUnitIdsToRetire { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheden die status 'niet gerealiseerd' moeten krijgen.")]
        public List<Guid> BuildingUnitIdsToNotRealize { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasCorrectedToRetired(
            BuildingId buildingId,
            IEnumerable<BuildingUnitId> buildingUnitsToRetire,
            IEnumerable<BuildingUnitId> buildingUnitsToNotRealize)
        {
            BuildingId = buildingId;
            BuildingUnitIdsToRetire = buildingUnitsToRetire?.Select(x => (Guid) x).ToList() ?? new List<Guid>();
            BuildingUnitIdsToNotRealize = buildingUnitsToNotRealize?.Select(x => (Guid) x).ToList() ?? new List<Guid>();
        }

        [JsonConstructor]
        private BuildingWasCorrectedToRetired(
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

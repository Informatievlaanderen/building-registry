namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventTags(EventTag.For.Sync)]
    [EventName("BuildingWasCorrectedToNotRealized")]
    [EventDescription("Het gebouw kreeg status 'niet gerealiseerd' (via correctie).")]
    public class BuildingWasCorrectedToNotRealized : IHasProvenance, ISetProvenance
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheden die status 'gehistoreerd' moeten krijgen.")]
        public List<Guid> BuildingUnitIdsToRetire { get; }

        [EventPropertyDescription("Interne GUID van de gebouweenheden die status 'niet gerealiseerd' moeten krijgen.")]
        public List<Guid> BuildingUnitIdsToNotRealize { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasCorrectedToNotRealized(
            BuildingId buildingId,
            IEnumerable<BuildingUnitId> buildingUnitIdsToRetire,
            IEnumerable<BuildingUnitId> buildingUnitIdsToNotRealize)
        {
            BuildingId = buildingId;

            BuildingUnitIdsToRetire = buildingUnitIdsToRetire?.Select(x => (Guid) x).ToList() ?? new List<Guid>();
            BuildingUnitIdsToNotRealize = buildingUnitIdsToNotRealize?.Select(x => (Guid) x).ToList() ?? new List<Guid>();
        }

        [JsonConstructor]
        private BuildingWasCorrectedToNotRealized(
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

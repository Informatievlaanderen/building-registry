namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingWasRemoved")]
    [EventDescription("Gebouw werd verwijderd")]
    public class BuildingWasRemoved : IHasProvenance, ISetProvenance
    {
        public Guid BuildingId { get; }
        public List<Guid> BuildingUnitIds { get; }
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasRemoved(
            BuildingId buildingId,
            IEnumerable<BuildingUnitId> buildingUnitIds)
        {
            BuildingId = buildingId;
            BuildingUnitIds = buildingUnitIds.Select(x => (Guid) x).ToList();
        }

        [JsonConstructor]
        private BuildingWasRemoved(
            Guid buildingId,
            IEnumerable<Guid> buildingUnitIds,
            ProvenanceData provenance)
            : this(
                new BuildingId(buildingId),
                buildingUnitIds.Select(x => new BuildingUnitId(x))) => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

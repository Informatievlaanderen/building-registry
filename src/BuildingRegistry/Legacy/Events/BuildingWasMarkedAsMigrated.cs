namespace BuildingRegistry.Legacy.Events
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [HideEvent]
    [Obsolete("CRAB events are obsolete.")]
    [EventName("BuildingWasMarkedAsMigrated")]
    [EventDescription("Het gebouw werd gemarkeerd als gemigreerd.")]
    public class BuildingWasMarkedAsMigrated : IHasProvenance, ISetProvenance, IMessage
    {
        [EventPropertyDescription("Interne GUID van het gebouw.")]
        public Guid BuildingId { get; }

        [EventPropertyDescription("Objectidentificator van het gebouw.")]
        public int PersistentLocalId { get; }

        [EventPropertyDescription("Metadata bij het event.")]
        public ProvenanceData Provenance { get; private set; }

        public BuildingWasMarkedAsMigrated(
            BuildingId buildingId,
            PersistentLocalId persistentLocalId)
        {
            BuildingId = buildingId;
            PersistentLocalId = persistentLocalId;
        }

        [JsonConstructor]
        private BuildingWasMarkedAsMigrated(
            Guid buildingId,
            int persistentLocalId,
            ProvenanceData provenance)
            : this (
                new BuildingId(buildingId),
                new PersistentLocalId(persistentLocalId))
            => ((ISetProvenance)this).SetProvenance(provenance.ToProvenance());

        void ISetProvenance.SetProvenance(Provenance provenance) => Provenance = new ProvenanceData(provenance);
    }
}

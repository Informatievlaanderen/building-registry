namespace BuildingRegistry.Tests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public static class BuildingWasRemovedV2Extensions
    {
        public static BuildingWasRemovedV2 WithBuildingPersistentLocalId(this BuildingWasRemovedV2 @event,
            int buildingPersistentLocalId)
        {
            var updatedEvent = new BuildingWasRemovedV2(
                new BuildingPersistentLocalId(buildingPersistentLocalId));
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }
    }
}

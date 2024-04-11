namespace BuildingRegistry.Tests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public static class BuildingUnitWasRemovedV2Extensions
    {
        public static BuildingUnitWasRemovedV2 WithBuildingUnitPersistentLocalId(this BuildingUnitWasRemovedV2 @event,
            int buildingUnitPersistentLocalId)
        {
            var updatedEvent = new BuildingUnitWasRemovedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId));
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }

        public static BuildingUnitWasRemovedV2 WithBuildingPersistentLocalId(this BuildingUnitWasRemovedV2 @event,
            int buildingPersistentLocalId)
        {
            var updatedEvent = new BuildingUnitWasRemovedV2(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId));
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }
    }
}

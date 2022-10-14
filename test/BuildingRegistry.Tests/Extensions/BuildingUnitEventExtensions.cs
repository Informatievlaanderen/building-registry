namespace BuildingRegistry.Tests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public static class BuildingUnitWasRealizedV2Extensions
    {
        public static BuildingUnitWasRealizedV2 WithBuildingUnitPersistentLocalId(this BuildingUnitWasRealizedV2 @event,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            var updatedEvent = new BuildingUnitWasRealizedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                buildingUnitPersistentLocalId);
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }

        public static CommonBuildingUnitWasAddedV2 WithBuildingUnitStatus(this CommonBuildingUnitWasAddedV2 @event,
            BuildingUnitStatus status)
        {
            var updatedEvent = new CommonBuildingUnitWasAddedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                status,
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod),
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.HasDeviation);
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }
    }
}

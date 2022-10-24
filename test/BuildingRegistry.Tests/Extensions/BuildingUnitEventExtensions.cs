namespace BuildingRegistry.Tests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public static class BuildingUnitWasPlannedV2Extensions
    {
        public static BuildingUnitWasPlannedV2 WithBuildingUnitPersistentLocalId(this BuildingUnitWasPlannedV2 @event,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            var updatedEvent = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                buildingUnitPersistentLocalId,
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod),
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitFunction.Parse(@event.Function),
                @event.HasDeviation);
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }

        public static BuildingUnitWasPlannedV2 WithFunction(this BuildingUnitWasPlannedV2 @event,
            BuildingUnitFunction function)
        {
            var updatedEvent = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod),
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                function,
                @event.HasDeviation);
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }
    }

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
    }

    public static class CommonBuildingUnitWasAddedV2Extensions
    {
        public static CommonBuildingUnitWasAddedV2 WithBuildingUnitPersistentLocalId(this CommonBuildingUnitWasAddedV2 @event,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            var updatedEvent = new CommonBuildingUnitWasAddedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                buildingUnitPersistentLocalId,
                BuildingUnitStatus.Parse(@event.BuildingUnitStatus),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod),
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                @event.HasDeviation);
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

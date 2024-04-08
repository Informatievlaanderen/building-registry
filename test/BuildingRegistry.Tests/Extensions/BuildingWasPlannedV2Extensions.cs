namespace BuildingRegistry.Tests.Extensions
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public static class BuildingWasPlannedV2Extensions
    {
        public static BuildingWasPlannedV2 WithGeometry(this BuildingWasPlannedV2 @event,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var updatedEvent = new BuildingWasPlannedV2(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                extendedWkbGeometry);
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }

        public static BuildingWasPlannedV2 WithBuildingPersistentLocalId(this BuildingWasPlannedV2 @event,
            int buildingPersistentLocalId)
        {
            var updatedEvent = new BuildingWasPlannedV2(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }

        public static void SetFixtureProvenance(this IBuildingEvent @event, Fixture fixture)
        {
            @event.SetProvenance(fixture.Create<Provenance>());
        }
    }

    // public static class BuildingWasMergedV2Extensions
    // {
    //     public static BuildingWasMerged WithSourceBuildingPersistentLocalId(this BuildingWasMerged @event,
    //         int sourceBuildingPersistentLocalId)
    //     {
    //         var updatedEvent = new BuildingWasMerged(
    //             new BuildingPersistentLocalId(sourceBuildingPersistentLocalId),
    //             new BuildingPersistentLocalId(@event.DestinationBuildingPersistentLocalId));
    //         ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());
    //
    //         return updatedEvent;
    //     }
    //
    //     public static BuildingWasMerged WithDestinationBuildingPersistentLocalId(this BuildingWasMerged @event,
    //         int destinationBuildingPersistentLocalId)
    //     {
    //         var updatedEvent = new BuildingWasMerged(
    //             new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
    //             new BuildingPersistentLocalId(destinationBuildingPersistentLocalId));
    //         ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());
    //
    //         return updatedEvent;
    //     }
    // }
}

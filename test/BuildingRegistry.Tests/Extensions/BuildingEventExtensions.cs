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

        public static void SetFixtureProvenance(this IBuildingEvent @event, Fixture fixture)
        {
            @event.SetProvenance(fixture.Create<Provenance>());
        }
    }
}

namespace BuildingRegistry.Tests.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;
    using System.Collections.Generic;

    public static class BuildingUnitWasMovedIntoBuildingExtensions
    {
        public static BuildingUnitWasMovedIntoBuilding WithAddressPersistentLocalIds(this BuildingUnitWasMovedIntoBuilding @event,
            IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            var updatedEvent = new BuildingUnitWasMovedIntoBuilding(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingPersistentLocalId(@event.SourceBuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                BuildingUnitStatus.Parse(@event.BuildingUnitStatus),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod),
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitFunction.Parse(@event.Function),
                @event.HasDeviation,
                addressPersistentLocalIds);
            ((ISetProvenance)updatedEvent).SetProvenance(@event.Provenance.ToProvenance());

            return updatedEvent;
        }
    }
}

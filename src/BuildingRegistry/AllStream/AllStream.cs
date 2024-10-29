namespace BuildingRegistry.AllStream
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using Events;

    public sealed class AllStream : AggregateRootEntity
    {
        public void CreateOsloSnapshots(
            IReadOnlyList<BuildingPersistentLocalId> buildingPersistentLocalIds,
            IReadOnlyList<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIds)
        {
            if(buildingPersistentLocalIds.Any())
                ApplyChange(new BuildingOsloSnapshotsWereRequested(buildingPersistentLocalIds));

            if(buildingUnitPersistentLocalIds.Any())
                ApplyChange(new BuildingUnitOsloSnapshotsWereRequested(buildingUnitPersistentLocalIds));
        }
    }
}

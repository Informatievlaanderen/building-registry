namespace BuildingRegistry.Projections.Integration.Building.LatestItem
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingLatestItemExtensions
    {
            public static async Task<BuildingLatestItem> FindAndUpdateBuilding(this IntegrationContext context,
                int buildingPersistentLocalId,
                long position,
                Action<BuildingLatestItem> updateFunc,
                CancellationToken ct)
            {
                var building = await context
                    .BuildingLatestItems
                    .FindAsync(buildingPersistentLocalId, cancellationToken: ct);

                if (building == null)
                    throw DatabaseItemNotFound(buildingPersistentLocalId);

                building.IdempotenceKey = position;

                updateFunc(building);

                return building;
            }

            private static ProjectionItemNotFoundException<BuildingLatestItemProjections> DatabaseItemNotFound(int buildingPersistentLocalId)
                => new ProjectionItemNotFoundException<BuildingLatestItemProjections>(buildingPersistentLocalId.ToString());
    }
}

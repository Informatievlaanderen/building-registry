namespace BuildingRegistry.Projections.Integration
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;

    public static class BuildingUnitLatestItemExtensions
    {
            public static async Task<BuildingUnitLatestItem> FindAndUpdateBuildingUnit(this IntegrationContext context,
                int buildingUnitPersistentLocalId,
                long position,
                Func<BuildingUnitLatestItem, Task> updateFunc,
                CancellationToken ct)
            {
                var buildingUnit = await context
                    .BuildingUnitLatestItems
                    .FindAsync(buildingUnitPersistentLocalId, cancellationToken: ct);

                if (buildingUnit == null)
                    throw DatabaseItemNotFound(buildingUnitPersistentLocalId);

                buildingUnit.IdempotenceKey = position;

                await updateFunc(buildingUnit);

                return buildingUnit;
            }

            private static ProjectionItemNotFoundException<BuildingUnitLatestItemProjections> DatabaseItemNotFound(int buildingUnitPersistentLocalId)
                => new ProjectionItemNotFoundException<BuildingUnitLatestItemProjections>(buildingUnitPersistentLocalId.ToString());
    }
}

namespace BuildingRegistry.Projections.Integration.BuildingUnit.LatestItem
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingUnitLatestItemExtensions
    {
            public static async Task<BuildingUnitLatestItem> FindAndUpdateBuildingUnit(this IntegrationContext context,
                int buildingUnitPersistentLocalId,
                Func<BuildingUnitLatestItem, Task> updateFunc,
                CancellationToken ct)
            {
                var buildingUnit = await context
                    .BuildingUnitLatestItems
                    .FindAsync(buildingUnitPersistentLocalId, cancellationToken: ct);

                if (buildingUnit == null)
                    throw DatabaseItemNotFound(buildingUnitPersistentLocalId);

                await updateFunc(buildingUnit);

                return buildingUnit;
            }

            public static async Task AddIdempotentBuildingUnitAddress(this IntegrationContext context,
                BuildingUnitLatestItem buildingUnit,
                int addressPersistentLocalId,
                CancellationToken ct)
            {
                var buildingUnitAddress = await context.BuildingUnitAddresses.FindAsync(
                    new object[] { buildingUnit.BuildingUnitPersistentLocalId, addressPersistentLocalId }, ct);

                if (buildingUnitAddress is null  || context.Entry(buildingUnitAddress).State == EntityState.Deleted)
                {
                    context.BuildingUnitAddresses.Add(new BuildingUnitAddress
                    {
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        AddressPersistentLocalId = addressPersistentLocalId
                    });
                }
            }

            public static async Task RemoveIdempotentBuildingUnitAddress(this IntegrationContext context,
                BuildingUnitLatestItem buildingUnit,
                int addressPersistentLocalId,
                CancellationToken ct)
            {
                var buildingUnitAddress = await context.BuildingUnitAddresses.FindAsync(
                    new object[] { buildingUnit.BuildingUnitPersistentLocalId, addressPersistentLocalId }, ct);

                if (buildingUnitAddress is not null)
                {
                    context.BuildingUnitAddresses.Remove(buildingUnitAddress);
                }
            }

            private static ProjectionItemNotFoundException<BuildingUnitLatestItemProjections> DatabaseItemNotFound(int buildingUnitPersistentLocalId)
                => new ProjectionItemNotFoundException<BuildingUnitLatestItemProjections>(buildingUnitPersistentLocalId.ToString());
    }
}

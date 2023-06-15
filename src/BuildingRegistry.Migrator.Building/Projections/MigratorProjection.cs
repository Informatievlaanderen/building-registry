namespace BuildingRegistry.Migrator.Building.Projections
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Api.BackOffice.Abstractions;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class MigratorProjection : ConnectedProjection<MigratorProjectionContext>
    {
        public MigratorProjection(ILogger logger, IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            When<Envelope<BuildingWasMigrated>>(async (_, message, ct) =>
            {
                await using var dbContext = await backOfficeContextFactory.CreateDbContextAsync(ct);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    foreach (var addressPersistentLocalId in buildingUnit.AddressPersistentLocalIds)
                    {
                        var relation = await dbContext.FindBuildingUnitAddressRelation(
                            new BuildingUnitPersistentLocalId(buildingUnit.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(addressPersistentLocalId),
                            ct);

                        if (relation is null)
                        {
                            relation = new BuildingUnitAddressRelation(message.Message.BuildingPersistentLocalId, buildingUnit.BuildingUnitPersistentLocalId, addressPersistentLocalId);
                            await dbContext.BuildingUnitAddressRelation.AddAsync(relation, ct);
                        }
                    }
                }

                await dbContext.SaveChangesAsync(ct);
            });
        }
    }
}

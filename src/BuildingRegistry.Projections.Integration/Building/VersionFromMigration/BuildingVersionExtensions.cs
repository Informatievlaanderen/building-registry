namespace BuildingRegistry.Projections.Integration.Building.VersionFromMigration
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;

    public static class BuildingVersionExtensions
    {
        public static async Task CreateNewBuildingVersion<T>(
            this IntegrationContext context,
            int buildingPersistentLocalId,
            Envelope<T> message,
            Action<BuildingVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var buildingVersion = await context.LatestPosition(buildingPersistentLocalId, ct);

            if (buildingVersion is null)
            {
                throw DatabaseItemNotFound(buildingPersistentLocalId);
            }

            var provenance = message.Message.Provenance;

            var newBuildingVersion = buildingVersion.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            await context
                .BuildingVersionsFromMigration
                .AddAsync(newBuildingVersion, ct);
        }

        private static async Task<BuildingVersion?> LatestPosition(
            this IntegrationContext context,
            int buildingPersistentLocalId,
            CancellationToken ct)
            => context
                   .BuildingVersionsFromMigration
                   .Local
                   .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .BuildingVersionsFromMigration
                   .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<BuildingVersionProjections> DatabaseItemNotFound(int buildingPersistentLocalId)
            => new(buildingPersistentLocalId.ToString());
    }
}

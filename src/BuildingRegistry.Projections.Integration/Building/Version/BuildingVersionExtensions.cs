namespace BuildingRegistry.Projections.Integration.Building.Version
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
            Guid buildingId,
            Envelope<T> message,
            Action<BuildingVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var buildingVersion = await context.LatestPosition(buildingId, ct);

            if (buildingVersion is null)
            {
                throw DatabaseItemNotFound(buildingId);
            }

            var provenance = message.Message.Provenance;

            var newBuildingVersion = buildingVersion.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                provenance.Timestamp,
                applyEventInfoOn);

            await context
                .BuildingVersions
                .AddAsync(newBuildingVersion, ct);
        }

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
                .BuildingVersions
                .AddAsync(newBuildingVersion, ct);
        }

        private static async Task<BuildingVersion?> LatestPosition(
            this IntegrationContext context,
            Guid buildingId,
            CancellationToken ct)
            => context
                   .BuildingVersions
                   .Local
                   .Where(x => x.BuildingId == buildingId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .BuildingVersions
                   .AsNoTracking()
                   .Where(x => x.BuildingId == buildingId)
                   .Include(x => x.BuildingUnits).ThenInclude(y => y.Addresses)
                   .Include(x => x.BuildingUnits).ThenInclude(y => y.Readdresses)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static async Task<BuildingVersion?> LatestPosition(
            this IntegrationContext context,
            int buildingPersistentLocalId,
            CancellationToken ct)
            => context
                   .BuildingVersions
                   .Local
                   .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .BuildingVersions
                   .AsNoTracking()
                   .Where(x => x.BuildingPersistentLocalId == buildingPersistentLocalId)
                   .Include(x => x.BuildingUnits).ThenInclude(y => y.Addresses)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<BuildingVersionProjections> DatabaseItemNotFound(Guid buildingId)
            => new(buildingId.ToString("D"));

        private static ProjectionItemNotFoundException<BuildingVersionProjections> DatabaseItemNotFound(int buildingPersistentLocalId)
            => new(buildingPersistentLocalId.ToString());
    }
}

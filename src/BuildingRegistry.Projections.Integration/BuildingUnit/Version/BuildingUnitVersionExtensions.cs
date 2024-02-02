namespace BuildingRegistry.Projections.Integration.BuildingUnit.Version
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

    public static class BuildingUnitVersionExtensions
    {
        public static async Task CreateNewBuildingUnitVersion<T>(
            this IntegrationContext context,
            Guid buildingUnitId,
            Envelope<T> message,
            Action<BuildingUnitVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var buildingUnitVersion = await context.LatestPosition(buildingUnitId, ct);

            if (buildingUnitVersion is null)
            {
                throw DatabaseItemNotFound(buildingUnitId);
            }

            var provenance = message.Message.Provenance;

            var newBuildingUnitVersion = buildingUnitVersion.CloneAndApplyEventInfo(
                message.Position,
                provenance.Timestamp,
                applyEventInfoOn);

            await context
                .BuildingUnitVersions
                .AddAsync(newBuildingUnitVersion, ct);
        }

        public static async Task CreateNewBuildingUnitVersion<T>(
            this IntegrationContext context,
            int buildingUnitPersistentLocalId,
            Envelope<T> message,
            Action<BuildingUnitVersion> applyEventInfoOn,
            CancellationToken ct) where T : IHasProvenance, IMessage
        {
            var buildingUnitVersion = await context.LatestPosition(buildingUnitPersistentLocalId, ct);

            if (buildingUnitVersion is null)
            {
                throw DatabaseItemNotFound(buildingUnitPersistentLocalId);
            }

            var provenance = message.Message.Provenance;

            var newBuildingUnitVersion = buildingUnitVersion.CloneAndApplyEventInfo(
                message.Position,
                provenance.Timestamp,
                applyEventInfoOn);

            await context
                .BuildingUnitVersions
                .AddAsync(newBuildingUnitVersion, ct);
        }

        public static async Task<BuildingUnitVersion?> LatestPosition(
            this IntegrationContext context,
            Guid buildingUnitId,
            CancellationToken ct)
            => context
                   .BuildingUnitVersions
                   .Local
                   .Where(x => x.BuildingUnitId == buildingUnitId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .BuildingUnitVersions
                   .Where(x => x.BuildingUnitId == buildingUnitId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static async Task<BuildingUnitVersion?> LatestPosition(
            this IntegrationContext context,
            int buildingUnitPersistentLocalId,
            CancellationToken ct)
            => context
                   .BuildingUnitVersions
                   .Local
                   .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .BuildingUnitVersions
                   .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<BuildingUnitVersionProjections> DatabaseItemNotFound(Guid buildingUnitId)
            => new(buildingUnitId.ToString("D"));

        private static ProjectionItemNotFoundException<BuildingUnitVersionProjections> DatabaseItemNotFound(int buildingUnitPersistentLocalId)
            => new(buildingUnitPersistentLocalId.ToString());
    }
}

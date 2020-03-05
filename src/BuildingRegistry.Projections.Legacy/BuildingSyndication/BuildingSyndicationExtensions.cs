namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;

    public static class BuildingSyndicationExtensions
    {
        public static async Task CreateNewBuildingSyndicationItem<T>(
            this LegacyContext context,
            Guid buildingId,
            Envelope<T> message,
            Action<BuildingSyndicationItem> applyEventInfoOn,
            CancellationToken ct)
        {
            var dummyApplyEventInfoOn = new Action<BuildingSyndicationItem, BuildingSyndicationItem>(
                (oldSyndicationItem, newSyndicationItem) => applyEventInfoOn(newSyndicationItem));

            await context.CreateNewBuildingSyndicationItem(
                buildingId,
                message,
                dummyApplyEventInfoOn,
                ct);
        }

        public static async Task CreateNewBuildingSyndicationItem<T>(
            this LegacyContext context,
            Guid buildingId,
            Envelope<T> message,
            Action<BuildingSyndicationItem, BuildingSyndicationItem> applyEventInfoOn,
            CancellationToken ct)
        {
            var buildingSyndicationItem = await context.LatestPosition(buildingId, ct);

            if (buildingSyndicationItem == null)
                throw DatabaseItemNotFound(buildingId);

            if (buildingSyndicationItem.Position >= message.Position)
                return;

            var dummyApplyEventInfoOn = new Action<BuildingSyndicationItem>(
                newSyndicationItem => applyEventInfoOn(buildingSyndicationItem, newSyndicationItem));

            var newBuildingSyndicationItem = buildingSyndicationItem.CloneAndApplyEventInfo(
                message.Position,
                message.EventName,
                message.Message is IHasProvenance x ? x.Provenance.Timestamp : Instant.MinValue,
                dummyApplyEventInfoOn);

            if (message.Message is IHasProvenance provenanceMessage)
                newBuildingSyndicationItem.ApplyProvenance(provenanceMessage.Provenance);

            newBuildingSyndicationItem.SetEventData(message.Message);

            await context
                .BuildingSyndication
                .AddAsync(newBuildingSyndicationItem, ct);
        }

        public static async Task<BuildingSyndicationItem> LatestPosition(
            this LegacyContext context,
            Guid buildingId,
            CancellationToken ct)
            => context
                   .BuildingSyndication
                   .Local
                   .Where(x => x.BuildingId == buildingId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefault()
               ?? await context
                   .BuildingSyndication
                   .Where(x => x.BuildingId == buildingId)
                   .Include(x => x.BuildingUnits).ThenInclude(y => y.Addresses)
                   .Include(x => x.BuildingUnits).ThenInclude(y => y.Readdresses)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        public static void ApplyProvenance(
            this BuildingSyndicationItem item,
            ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Reason = provenance.Reason;
        }

        public static void SetEventData<T>(this BuildingSyndicationItem syndicationItem, T message)
            => syndicationItem.EventDataAsXml = message.ToXml(message.GetType().Name).ToString(SaveOptions.DisableFormatting);

        private static ProjectionItemNotFoundException<BuildingSyndicationProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<BuildingSyndicationProjections>(addressId.ToString("D"));
    }
}

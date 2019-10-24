namespace BuildingRegistry.Projections.Syndication.Parcel
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using System.Threading;
    using System.Threading.Tasks;

    public class BuildingParcelLatestProjection : AtomEntryProjectionHandlerModule<ParcelEvent, SyndicationContent<Parcel>, SyndicationContext>
    {
        public BuildingParcelLatestProjection()
        {
            When(ParcelEvent.ParcelWasRegistered, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRemoved, RemoveSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRealized, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasCorrectedToRealized, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasRetired, AddSyndicationItemEntry);
            When(ParcelEvent.ParcelWasCorrectedToRetired, AddSyndicationItemEntry);

            When(ParcelEvent.ParcelAddressWasAttached, DoNothing);
            When(ParcelEvent.ParcelAddressWasDetached, DoNothing);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationContent<Parcel>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .BuildingParcelLatestItems
                .FindAsync(entry.Content.Object.ParcelId);

            if (latestItem == null)
            {
                latestItem = new BuildingParcelLatestItem
                {
                    ParcelId = entry.Content.Object.ParcelId,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    CaPaKey = entry.Content.Object.Identificator?.ObjectId,
                    IsComplete = entry.Content.Object.IsComplete,
                    Status = entry.Content.Object.Status,
                };

                await context
                      .BuildingParcelLatestItems
                      .AddAsync(latestItem, ct);
            }
            else
            {
                latestItem.Version = entry.Content.Object.Identificator?.Versie;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.CaPaKey = entry.Content.Object.Identificator?.ObjectId;
                latestItem.IsComplete = entry.Content.Object.IsComplete;
                latestItem.Status = entry.Content.Object.Status;
            }
        }

        private static async Task RemoveSyndicationItemEntry(AtomEntry<SyndicationContent<Parcel>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .BuildingParcelLatestItems
                .FindAsync(entry.Content.Object.ParcelId);

            latestItem.Version = entry.Content.Object.Identificator?.Versie;
            latestItem.Position = long.Parse(entry.FeedEntry.Id);
            latestItem.CaPaKey = entry.Content.Object.Identificator?.ObjectId;
            latestItem.IsComplete = entry.Content.Object.IsComplete;
            latestItem.Status = entry.Content.Object.Status;
            latestItem.IsRemoved = true;
        }

        private static Task DoNothing(AtomEntry<SyndicationContent<Parcel>> entry, SyndicationContext context, CancellationToken ct) => Task.CompletedTask;
    }
}

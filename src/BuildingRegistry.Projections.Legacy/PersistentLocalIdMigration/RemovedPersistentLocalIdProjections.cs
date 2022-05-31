namespace BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using BuildingRegistry.Legacy.Events;

    // Artifact from INIT, can be removed once the EDIT-api is active
    public class RemovedPersistentLocalIdProjections : ConnectedProjection<LegacyContext>
    {
        public RemovedPersistentLocalIdProjections()
        {
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) =>
            {
                var id = await context.RemovedPersistentLocalIds.FindAsync(message.Message.PersistentLocalId, cancellationToken: ct);

                if (id != null)
                    return;

                await context
                    .RemovedPersistentLocalIds
                    .AddAsync(
                        new RemovedPersistentLocalId
                        {
                            PersistentLocalId = message.Message.PersistentLocalId,
                            Reason = message.Message.Reason,
                            BuildingId = message.Message.BuildingId
                        },
                        ct);
            });
        }
    }
}

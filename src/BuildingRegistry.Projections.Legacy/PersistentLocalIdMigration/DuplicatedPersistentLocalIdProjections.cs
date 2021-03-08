namespace BuildingRegistry.Projections.Legacy.PersistentLocalIdMigration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;

    // Artifact from INIT, can be removed once the EDIT-api is active
    public class DuplicatedPersistentLocalIdProjections : ConnectedProjection<LegacyContext>
    {
        public DuplicatedPersistentLocalIdProjections()
        {
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) =>
            {
                var id = await context.DuplicatedPersistentLocalIds.FindAsync(message.Message.DuplicatePersistentLocalId, cancellationToken: ct);

                if (id != null)
                    return;

                await context
                    .DuplicatedPersistentLocalIds
                    .AddAsync(
                        new DuplicatedPersistentLocalId
                        {
                            DuplicatePersistentLocalId = message.Message.DuplicatePersistentLocalId,
                            BuildingId = message.Message.BuildingId,
                            OriginalPersistentLocalId = message.Message.OriginalPersistentLocalId
                        },
                        ct);
            });
        }
    }
}

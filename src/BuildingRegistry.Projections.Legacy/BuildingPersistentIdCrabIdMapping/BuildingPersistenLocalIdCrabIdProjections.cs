namespace BuildingRegistry.Projections.Legacy.BuildingPersistentIdCrabIdMapping
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using Building.Events.Crab;
    using ValueObjects;

    public class BuildingPersistenLocalIdCrabIdProjections : ConnectedProjection<LegacyContext>
    {
        public BuildingPersistenLocalIdCrabIdProjections()
        {
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .BuildingPersistentIdCrabIdMappings
                    .AddAsync(
                        new BuildingPersistentLocalIdCrabIdMapping
                        {
                            BuildingId = message.Message.BuildingId,
                        },
                        ct);
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingPersistentIdCrabIdMappings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                item.PersistentLocalId = message.Message.PersistentLocalId;
            });

            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) =>
            {
                Guid buildingId = BuildingId.CreateFor(new CrabTerrainObjectId(message.Message.TerrainObjectId));
                var item = await context.BuildingPersistentIdCrabIdMappings.FindAsync(buildingId, cancellationToken: ct);
                item.CrabTerrainObjectId = message.Message.TerrainObjectId;
                item.CrabIdentifierTerrainObject = message.Message.IdentifierTerrainObject;
            });
        }
    }
}

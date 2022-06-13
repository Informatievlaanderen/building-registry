namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Commands;
    using SqlStreamStore;

    public sealed class BuildingCommandHandlerModule : CommandHandlerModule
    {
        public BuildingCommandHandlerModule(
            Func<IBuildings> buildingRepository,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            ProvenanceFactory<Building> provenanceFactory)
        {
            For<MigrateBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddEventHash<MigrateBuilding, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetOptionalAsync(streamId, ct);

                    if (building.HasValue)
                    {
                        throw new AggregateSourceException($"Building with id {message.Command.BuildingPersistentLocalId} already exists");
                    }

                    var newBuilding = Building.MigrateBuilding(
                        message.Command.BuildingId,
                        message.Command.BuildingPersistentLocalId,
                        message.Command.BuildingPersistentLocalIdAssignmentDate,
                        message.Command.BuildingStatus,
                        message.Command.BuildingGeometry,
                        message.Command.IsRemoved,
                        message.Command.BuildingUnits);

                    buildingRepository().Add(streamId, newBuilding);
                });

            For<PlanBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddEventHash<PlanBuilding, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetOptionalAsync(streamId, ct);

                    if (building.HasValue)
                    {
                        throw new AggregateSourceException($"Building with id {message.Command.BuildingPersistentLocalId} already exists");
                    }

                    var newBuilding = Building.Plan(
                        message.Command.BuildingPersistentLocalId,
                        message.Command.Geometry);

                    buildingRepository().Add(streamId, newBuilding);
                });
        }
    }
}

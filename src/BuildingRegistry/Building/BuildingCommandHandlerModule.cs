namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
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
            IBuildingFactory buildingFactory,
            Func<IBuildings> buildingRepository,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            Func<ISnapshotStore> getSnapshotStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IProvenanceFactory<Building> provenanceFactory)
        {
            For<MigrateBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
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
                        buildingFactory,
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
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
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
                        buildingFactory,
                        message.Command.BuildingPersistentLocalId,
                        message.Command.Geometry);

                    buildingRepository().Add(streamId, newBuilding);
                });

            For<ChangeBuildingOutline>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ChangeBuildingOutline, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.ChangeOutlining(message.Command.Geometry);
                });

            For<PlaceBuildingUnderConstruction>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<PlaceBuildingUnderConstruction, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.PlaceUnderConstruction();
                });

            For<CorrectBuildingPlaceUnderConstruction>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingPlaceUnderConstruction, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingPlaceUnderConstruction();
                });

            For<RealizeBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RealizeBuilding, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.RealizeConstruction();
                });

            For<CorrectBuildingRealization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingRealization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectRealizeConstruction();
                });

            For<NotRealizeBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<NotRealizeBuilding, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.NotRealizeConstruction();
                });

            For<CorrectBuildingNotRealization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingNotRealization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectNotRealizeConstruction();
                });
        }
    }
}

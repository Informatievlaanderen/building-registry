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

    public sealed class BuildingUnitCommandHandlerModule : CommandHandlerModule
    {
        public BuildingUnitCommandHandlerModule(
            Func<IBuildings> buildingRepository,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            Func<ISnapshotStore> getSnapshotStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IProvenanceFactory<Building> provenanceFactory,
            IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            For<PlanBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<PlanBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.PlanBuildingUnit(
                        addCommonBuildingUnit,
                        message.Command.BuildingUnitPersistentLocalId,
                        message.Command.PositionGeometryMethod,
                        message.Command.Position,
                        message.Command.Function,
                        message.Command.HasDeviation);
                });

            For<RealizeBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RealizeBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.RealizeBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitRealization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitRealization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectRealizeBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<NotRealizeBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<NotRealizeBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.NotRealizeBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitNotRealization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitNotRealization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectNotRealizeBuildingUnit(addCommonBuildingUnit, message.Command.BuildingUnitPersistentLocalId);
                });

            For<RetireBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RetireBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.RetireBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });
        }
    }
}

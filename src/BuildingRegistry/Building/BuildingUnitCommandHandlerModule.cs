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
            IProvenanceFactory<Building> provenanceFactory)
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

                    building.CorrectNotRealizeBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
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

            For<CorrectBuildingUnitRetirement>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitRetirement, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectRetiredBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitPosition>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitPosition, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectPositionBuildingUnit(message.Command.BuildingUnitPersistentLocalId, message.Command.PositionGeometryMethod, message.Command.Position);
                });

            For<RemoveBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RemoveBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.RemoveBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<RegularizeBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RegularizeBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.RegularizeBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<DeregulateBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DeregulateBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.DeregulateBuildingUnit(message.Command.BuildingUnitPersistentLocalId);
                });

            For<AttachAddressToBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<AttachAddressToBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.AttachAddressToBuildingUnit(message.Command.BuildingUnitPersistentLocalId, message.Command.AddressPersistentLocalId);
                });

            For<DetachAddressFromBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressFromBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.DetachAddressFromBuildingUnit(message.Command.BuildingUnitPersistentLocalId, message.Command.AddressPersistentLocalId);
                });

            For<DetachAddressFromBuildingUnitBecauseAddressWasRemoved>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressFromBuildingUnitBecauseAddressWasRemoved, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.DetachAddressFromBuildingUnitBecauseAddressWasRemoved(message.Command.BuildingUnitPersistentLocalId, message.Command.AddressPersistentLocalId);
                });

            For<DetachAddressFromBuildingUnitBecauseAddressWasRejected>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressFromBuildingUnitBecauseAddressWasRejected, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.DetachAddressFromBuildingUnitBecauseAddressWasRejected(message.Command.BuildingUnitPersistentLocalId, message.Command.AddressPersistentLocalId);
                });

            For<DetachAddressFromBuildingUnitBecauseAddressWasRetired>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<DetachAddressFromBuildingUnitBecauseAddressWasRetired, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.DetachAddressFromBuildingUnitBecauseAddressWasRetired(message.Command.BuildingUnitPersistentLocalId, message.Command.AddressPersistentLocalId);
                });
        }
    }
}

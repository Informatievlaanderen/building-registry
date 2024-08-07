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
            IAddCommonBuildingUnit addCommonBuildingUnit,
            IAddresses addresses)
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

            For<CorrectBuildingUnitRealization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitRealization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitRealization(message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitNotRealization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitNotRealization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitNotRealization(addCommonBuildingUnit, message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitRetirement>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitRetirement, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitRetirement(addCommonBuildingUnit, message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitRemoval>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitRemoval, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitRemoval(addCommonBuildingUnit, message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitPosition>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitPosition, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitPosition(message.Command.BuildingUnitPersistentLocalId, message.Command.PositionGeometryMethod, message.Command.Position);
                });

            For<CorrectBuildingUnitRegularization>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitRegularization, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitRegularization(message.Command.BuildingUnitPersistentLocalId);
                });

            For<CorrectBuildingUnitDeregulation>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<CorrectBuildingUnitDeregulation, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.CorrectBuildingUnitDeregulation(message.Command.BuildingUnitPersistentLocalId);
                });

            For<AttachAddressToBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<AttachAddressToBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.AttachAddressToBuildingUnit(addresses, message.Command.BuildingUnitPersistentLocalId, message.Command.AddressPersistentLocalId);
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

            For<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger(
                        message.Command.BuildingUnitPersistentLocalId,
                        message.Command.NewAddressPersistentLocalId,
                        message.Command.PreviousAddressPersistentLocalId);
                });

            For<RealizeUnplannedBuildingUnit>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<RealizeUnplannedBuildingUnit, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.BuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.RealizeUnplannedBuildingUnit(
                        message.Command.BuildingUnitPersistentLocalId,
                        message.Command.AddressPersistentLocalId);
                });

            For<MoveBuildingUnitIntoBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<MoveBuildingUnitIntoBuilding, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var buildings = buildingRepository();

                    var destinationStreamId = new BuildingStreamId(message.Command.DestinationBuildingPersistentLocalId);
                    var building = await buildings.GetAsync(destinationStreamId, ct);

                    var sourceStreamId = new BuildingStreamId(message.Command.SourceBuildingPersistentLocalId);
                    var sourceBuilding = await buildings.GetAsync(sourceStreamId, ct);

                    building.MoveBuildingUnitInto(sourceBuilding,
                        message.Command.BuildingUnitPersistentLocalId,
                        addCommonBuildingUnit);
                });

            For<MoveBuildingUnitOutOfBuilding>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer, getSnapshotStore)
                .AddEventHash<MoveBuildingUnitOutOfBuilding, Building>(getUnitOfWork)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) =>
                {
                    var streamId = new BuildingStreamId(message.Command.SourceBuildingPersistentLocalId);
                    var building = await buildingRepository().GetAsync(streamId, ct);

                    building.MoveBuildingUnitOutOf(
                        message.Command.DestinationBuildingPersistentLocalId,
                        message.Command.BuildingUnitPersistentLocalId);
                });
        }
    }
}

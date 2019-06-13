namespace BuildingRegistry.Building
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Commands;
    using Commands.Crab;
    using SqlStreamStore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using ValueObjects;

    public sealed class BuildingCommandHandlerModule : CommandHandlerModule
    {
        private readonly Func<IBuildings> _getBuildings;
        private readonly IOsloIdGenerator _osloIdGenerator;

        public BuildingCommandHandlerModule(
            Func<IBuildings> getBuildings,
            Func<ConcurrentUnitOfWork> getUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IOsloIdGenerator osloIdGenerator,
            BuildingProvenanceFactory provenanceFactory)
        {
            _getBuildings = getBuildings;
            _osloIdGenerator = osloIdGenerator;

            For<ImportTerrainObjectFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportTerrainObject(message, ct); });

            For<ImportBuildingStatusFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportBuildingStatus(message, ct); });

            For<ImportBuildingGeometryFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportBuildingGeometry(message, ct); });

            For<ImportTerrainObjectHouseNumberFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportTerrainObjectHouseNumber(message, ct); });

            For<ImportHouseNumberStatusFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportHouseNumberStatus(message, ct); });

            For<ImportHouseNumberPositionFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportHouseNumberPosition(message, ct); });

            For<ImportSubaddressFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportSubaddress(message, ct); });

            For<ImportSubaddressStatusFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportSubaddressStatus(message, ct); });

            For<ImportSubaddressPositionFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .AddProvenance(getUnitOfWork, provenanceFactory)
                .Handle(async (message, ct) => { await ImportSubaddressPosition(message, ct); });

            For<AssignOsloIdForCrabTerrainObjectId>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .Handle(async (message, ct) => { await AssignOsloIdForCrabTerrainObjectId(message, ct); });

            For<ImportReaddressingHouseNumberFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .Handle(async (message, ct) => { await ImportReaddressingHouseNumber(message, ct); });

            For<ImportReaddressingSubaddressFromCrab>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .Handle(async (message, ct) => { await ImportReaddressingSubaddress(message, ct); });

            For<RequestOsloIdsForCrabTerrainObjectId>()
                .AddSqlStreamStore(getStreamStore, getUnitOfWork, eventMapping, eventSerializer)
                .Handle(async (message, ct) => { await RequestOsloIdsForCrabTerrainObjectId(message, ct); });
        }

        public async Task ImportSubaddressPosition(CommandMessage<ImportSubaddressPositionFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportSubaddressPositionFromCrab(
                message.Command.TerrainObjectId,
                message.Command.TerrainObjectHouseNumberId,
                message.Command.AddressPositionId,
                message.Command.SubaddressId,
                message.Command.AddressPosition,
                message.Command.AddressPositionOrigin,
                message.Command.AddressNature,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportSubaddressStatus(CommandMessage<ImportSubaddressStatusFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportSubaddressStatusFromCrab(
                message.Command.TerrainObjectId,
                message.Command.TerrainObjectHouseNumberId,
                message.Command.SubaddressStatusId,
                message.Command.SubaddressId,
                message.Command.SubaddressStatus,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportSubaddress(CommandMessage<ImportSubaddressFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportSubaddressFromCrab(
                message.Command.TerrainObjectId,
                message.Command.TerrainObjectHouseNumberId,
                message.Command.SubaddressId,
                message.Command.HouseNumberId,
                message.Command.BoxNumber,
                message.Command.BoxNumberType,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportHouseNumberPosition(CommandMessage<ImportHouseNumberPositionFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportHouseNumberPositionFromCrab(
                message.Command.TerrainObjectId,
                message.Command.TerrainObjectHouseNumberId,
                message.Command.AddressPositionId,
                message.Command.HouseNumberId,
                message.Command.AddressPosition,
                message.Command.AddressPositionOrigin,
                message.Command.AddressNature,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportHouseNumberStatus(CommandMessage<ImportHouseNumberStatusFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportHouseNumberStatusFromCrab(
                message.Command.TerrainObjectId,
                message.Command.TerrainObjectHouseNumberId,
                message.Command.HouseNumberStatusId,
                message.Command.HouseNumberId,
                message.Command.AddressStatus,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportTerrainObjectHouseNumber(CommandMessage<ImportTerrainObjectHouseNumberFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportTerrainObjectHouseNumberFromCrab(
                message.Command.TerrainObjectHouseNumberId,
                message.Command.TerrainObjectId,
                message.Command.HouseNumberId,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportBuildingGeometry(CommandMessage<ImportBuildingGeometryFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportBuildingGeometryFromCrab(
                message.Command.BuildingGeometryId,
                message.Command.TerrainObjectId,
                message.Command.BuildingGeometry,
                message.Command.BuildingGeometryMethod,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportBuildingStatus(CommandMessage<ImportBuildingStatusFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportBuildingStatusFromCrab(
                message.Command.BuildingStatusId,
                message.Command.TerrainObjectId,
                message.Command.BuildingStatus,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task ImportTerrainObject(CommandMessage<ImportTerrainObjectFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetOptionalAsync(buildingId.ToString(), ct);

            if (!building.HasValue)
            {
                building = new Optional<Building>(
                    Building.Register(new BuildingId(buildingId)));

                buildings.Add(buildingId.ToString(), building.Value);
            }

            building.Value.ImportTerrainObjectFromCrab(
                message.Command.TerrainObjectId,
                message.Command.IdentifierTerrainObject,
                message.Command.TerrainObjectNatureCode,
                message.Command.XCoordinate,
                message.Command.YCoordinate,
                message.Command.BuildingNature,
                message.Command.Lifetime,
                message.Command.Timestamp,
                message.Command.Operator,
                message.Command.Modification,
                message.Command.Organisation);
        }

        public async Task AssignOsloIdForCrabTerrainObjectId(CommandMessage<AssignOsloIdForCrabTerrainObjectId> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.AssignOsloIdForCrabTerrainObjectId(
                message.Command.TerrainObjectId,
                message.Command.OsloId,
                message.Command.AssignmentDate,
                message.Command.BuildingUnitOsloIds,
                _osloIdGenerator);
        }

        public async Task ImportReaddressingHouseNumber(CommandMessage<ImportReaddressingHouseNumberFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportReaddressHouseNumberFromCrab(
                message.Command.TerrainObjectId,
                message.Command.ReaddressingId,
                message.Command.BeginDate,
                message.Command.OldTerrainObjectHouseNumberId,
                message.Command.OldAddressNature,
                message.Command.OldHouseNumberId,
                message.Command.NewTerrainObjectHouseNumberId,
                message.Command.NewAddressNature,
                message.Command.NewHouseNumberId);
        }

        public async Task ImportReaddressingSubaddress(CommandMessage<ImportReaddressingSubaddressFromCrab> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.ImportReaddressSubaddressFromCrab(
                message.Command.TerrainObjectId,
                message.Command.ReaddressingId,
                message.Command.BeginDate,
                message.Command.OldTerrainObjectHouseNumberId,
                message.Command.OldAddressNature,
                message.Command.OldSubaddressId,
                message.Command.NewTerrainObjectHouseNumberId,
                message.Command.NewAddressNature,
                message.Command.NewSubaddressId);
        }

        public async Task RequestOsloIdsForCrabTerrainObjectId(
            CommandMessage<RequestOsloIdsForCrabTerrainObjectId> message, CancellationToken ct)
        {
            var buildings = _getBuildings();
            var buildingId = message.Command.TerrainObjectId.CreateDeterministicId();
            var building = await buildings.GetAsync(buildingId.ToString(), ct);

            building.AssignOsloIds(_osloIdGenerator);
        }
    }
}

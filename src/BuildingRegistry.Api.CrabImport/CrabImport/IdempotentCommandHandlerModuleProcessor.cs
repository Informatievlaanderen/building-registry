namespace BuildingRegistry.Api.CrabImport.CrabImport
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands.Crab;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using SqlStreamStore;

    public class IdempotentCommandHandlerModuleProcessor : IIdempotentCommandHandlerModuleProcessor
    {
        private readonly ILogger<IdempotentCommandHandlerModuleProcessor> _logger;
        private readonly ConcurrentUnitOfWork _concurrentUnitOfWork;
        private readonly BuildingCommandHandlerModule _buildingCommandHandlerModule;
        private readonly Func<IHasCrabProvenance, Building, Provenance> _provenanceFactory;

        public IdempotentCommandHandlerModuleProcessor(
            ILogger<IdempotentCommandHandlerModuleProcessor> logger,
            ILifetimeScope container,
            ConcurrentUnitOfWork concurrentUnitOfWork,
            Func<IStreamStore> getStreamStore,
            EventMapping eventMapping,
            EventSerializer eventSerializer,
            IOsloIdGenerator osloIdGenerator,
            BuildingProvenanceFactory provenanceFactory)
        {
            _logger = logger;
            _concurrentUnitOfWork = concurrentUnitOfWork;
            _provenanceFactory = provenanceFactory.CreateFrom;

            _buildingCommandHandlerModule = new BuildingCommandHandlerModule(
                container.Resolve<Func<IBuildings>>(),
                () => concurrentUnitOfWork,
                getStreamStore,
                eventMapping,
                eventSerializer,
                osloIdGenerator,
                provenanceFactory);
        }

        public async Task<CommandMessage> Process(
            dynamic commandToProcess,
            IDictionary<string, object> metadata,
            int currentPosition,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CommandMessage message = null;
            var stopwatch = Stopwatch.StartNew();
            switch (commandToProcess)
            {
                case ImportTerrainObjectFromCrab command:
                    var commandTerrainObjectMessage = new CommandMessage<ImportTerrainObjectFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportTerrainObject(commandTerrainObjectMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandTerrainObjectMessage, _provenanceFactory, currentPosition);
                    message = commandTerrainObjectMessage;
                    break;

                case ImportBuildingStatusFromCrab command:
                    var commandBuildingStatusMessage = new CommandMessage<ImportBuildingStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportBuildingStatus(commandBuildingStatusMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandBuildingStatusMessage, _provenanceFactory, currentPosition);
                    message = commandBuildingStatusMessage;
                    break;

                case ImportBuildingGeometryFromCrab command:
                    var commandBuildingGeometryMessage = new CommandMessage<ImportBuildingGeometryFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportBuildingGeometry(commandBuildingGeometryMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandBuildingGeometryMessage, _provenanceFactory, currentPosition);
                    message = commandBuildingGeometryMessage;
                    break;

                case ImportTerrainObjectHouseNumberFromCrab command:
                    var commandTerrainObjectHouseNumber = new CommandMessage<ImportTerrainObjectHouseNumberFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportTerrainObjectHouseNumber(commandTerrainObjectHouseNumber, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandTerrainObjectHouseNumber, _provenanceFactory, currentPosition);
                    message = commandTerrainObjectHouseNumber;
                    break;

                case ImportHouseNumberStatusFromCrab command:
                    var commandHouseNumberStatus = new CommandMessage<ImportHouseNumberStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportHouseNumberStatus(commandHouseNumberStatus, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberStatus, _provenanceFactory, currentPosition);
                    message = commandHouseNumberStatus;
                    break;

                case ImportHouseNumberPositionFromCrab command:
                    var commandHouseNumberPosition = new CommandMessage<ImportHouseNumberPositionFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportHouseNumberPosition(commandHouseNumberPosition, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandHouseNumberPosition, _provenanceFactory, currentPosition);
                    message = commandHouseNumberPosition;
                    break;

                case ImportSubaddressFromCrab command:
                    var commandSubaddressMessage = new CommandMessage<ImportSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportSubaddress(commandSubaddressMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressMessage, _provenanceFactory, currentPosition);
                    message = commandSubaddressMessage;
                    break;
                case ImportSubaddressStatusFromCrab command:
                    var commandSubaddressStatusMessage = new CommandMessage<ImportSubaddressStatusFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportSubaddressStatus(commandSubaddressStatusMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressStatusMessage, _provenanceFactory, currentPosition);
                    message = commandSubaddressStatusMessage;
                    break;

                case ImportSubaddressPositionFromCrab command:
                    var commandSubaddressPositionMessage = new CommandMessage<ImportSubaddressPositionFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportSubaddressPosition(commandSubaddressPositionMessage, cancellationToken);
                    AddProvenancePipe.AddProvenance(() => _concurrentUnitOfWork, commandSubaddressPositionMessage, _provenanceFactory, currentPosition);
                    message = commandSubaddressPositionMessage;
                    break;

                case AssignOsloIdForCrabTerrainObjectId command:
                    var commandAssignOsloId = new CommandMessage<AssignOsloIdForCrabTerrainObjectId>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.AssignOsloIdForCrabTerrainObjectId(commandAssignOsloId, cancellationToken);
                    message = commandAssignOsloId;
                    break;

                case RequestOsloIdsForCrabTerrainObjectId command:
                    var commandRequestOsloId = new CommandMessage<RequestOsloIdsForCrabTerrainObjectId>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.RequestOsloIdsForCrabTerrainObjectId(commandRequestOsloId, cancellationToken);
                    message = commandRequestOsloId;
                    break;

                case ImportReaddressingHouseNumberFromCrab command:
                    var commandReaddressHouseNumber = new CommandMessage<ImportReaddressingHouseNumberFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportReaddressingHouseNumber(commandReaddressHouseNumber, cancellationToken);
                    message = commandReaddressHouseNumber;
                    break;

                case ImportReaddressingSubaddressFromCrab command:
                    var commandReaddressSubaddress = new CommandMessage<ImportReaddressingSubaddressFromCrab>(command.CreateCommandId(), command, metadata);
                    await _buildingCommandHandlerModule.ImportReaddressingSubaddress(commandReaddressSubaddress, cancellationToken);
                    message = commandReaddressSubaddress;
                    break;

                default:
                    throw new NotImplementedException("Command to import is not recognized");
            }

            stopwatch.Stop();

            _logger.LogTrace($"Took {stopwatch.ElapsedMilliseconds}ms to process command {commandToProcess}");
            return message;
        }
    }
}
